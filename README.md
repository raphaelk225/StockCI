# StockCI

API REST de gestion de stock (produits, catégories, utilisateurs, mouvements de stock) en **ASP.NET Core 8** avec **MongoDB Atlas**, déployée sur **Azure App Service (Linux)**.

## Sommaire

- [Prérequis](#prérequis)
- [Configuration](#configuration)
- [Lancer en local](#lancer-en-local)
- [1. Préparer MongoDB Atlas](#1-préparer-mongodb-atlas)
- [2. Créer les ressources Azure](#2-créer-les-ressources-azure)
- [3. Configurer les variables d'environnement](#3-configurer-les-variables-denvironnement)
- [4. Autoriser Azure dans Atlas](#4-autoriser-azure-dans-atlas)
- [5. Déployer](#5-déployer)
- [6. Vérifier](#6-vérifier)
- [Dépannage](#dépannage)

---

## Prérequis

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az login` effectué)
- Un compte [MongoDB Atlas](https://cloud.mongodb.com)
- `zip` (pour empaqueter le build)

## Configuration

L'application lit deux variables d'environnement (validées au démarrage dans `Program.cs`) :

| Variable | Description | Exemple |
|---|---|---|
| `StockManagementDatabase__ConnectionString` | Chaîne de connexion MongoDB | `mongodb://user:pass@host1:27017,...` |
| `StockManagementDatabase__DatabaseName` | Nom de la base | `Stock_Management` |

Le `__` (double underscore) correspond à la section imbriquée `StockManagementDatabase:ConnectionString`. Sur App Service Linux, le `:` n'est pas autorisé dans les noms de variables : il faut utiliser `__`.

En local, ces variables sont chargées depuis un fichier `api-stockci/.env` (ignoré par git). Sur Azure, elles sont définies dans les *App settings*. Grâce à `DotNetEnv.Env.NoClobber()`, une variable déjà présente dans l'environnement n'est jamais écrasée par le `.env`.

## Lancer en local

```bash
cd api-stockci
cp .env.example .env   # puis renseigner la connection string
dotnet run
```

Swagger est disponible sur http://localhost:5033/swagger (uniquement en environnement `Development`).

---

## 1. Préparer MongoDB Atlas

1. **Créer un cluster** : *Create → Cluster*, tier **M0 (Free)**, provider **Azure**, région proche de l'App Service (ex. *West Europe*).
2. **Créer un utilisateur** : *Security → Database Access → Add New Database User*
   - Authentification par mot de passe
   - Rôle `readWrite` sur la base `Stock_Management`
   - Éviter les caractères spéciaux (`@ : / ? # %`) dans le mot de passe, ou les encoder en URL (`@` → `%40`).
3. **Récupérer la connection string** : *Connect → Drivers → C# / .NET*.

### Format de la connection string

Atlas propose une URL `mongodb+srv://...`, qui nécessite des requêtes DNS **SRV** et **TXT**. Certains réseaux (box, FAI) bloquent les requêtes TXT, ce qui provoque l'erreur `DnsResponseException ... IN TXT ... timed out`.

Pour éviter toute dépendance au DNS, on utilise le **format standard** avec les hôtes explicites :

```
mongodb://<USER>:<PASSWORD>@<HOST-00>:27017,<HOST-01>:27017,<HOST-02>:27017/?tls=true&authSource=admin&retryWrites=true&w=majority
```

Pour obtenir les trois hôtes du cluster :

```bash
dig +short SRV _mongodb._tcp.<cluster>.<id>.mongodb.net
# ex. : 0 0 27017 ac-xxxxxxx-shard-00-00.<id>.mongodb.net.
```

Autre solution : dans Atlas, *Connect → Drivers*, choisir une ancienne version du driver C# pour afficher la chaîne `mongodb://` complète.

## 2. Créer les ressources Azure

```bash
az group create -n rg-stockci -l francecentral

az appservice plan create -n plan-stockci -g rg-stockci --sku B1 --is-linux

az webapp create -n stockci-api -g rg-stockci -p plan-stockci --runtime "DOTNETCORE:8.0"
```

Le nom `stockci-api` doit être unique au niveau mondial : l'URL de l'API sera `https://stockci-api.azurewebsites.net`.

## 3. Configurer les variables d'environnement

```bash
az webapp config appsettings set -g rg-stockci -n stockci-api --settings \
  'StockManagementDatabase__ConnectionString=mongodb://<USER>:<PASSWORD>@<HOST-00>:27017,<HOST-01>:27017,<HOST-02>:27017/?tls=true&authSource=admin&retryWrites=true&w=majority' \
  'StockManagementDatabase__DatabaseName=Stock_Management'
```

- Les guillemets simples autour de chaque paire `nom=valeur` empêchent le shell d'interpréter `&` et `?`.
- Ne pas mettre de guillemets **à l'intérieur** de la valeur.
- Même chose via le portail : *App Service → Settings → Environment variables → App settings*.

Vérifier que les variables sont bien présentes (sans afficher les valeurs) :

```bash
az webapp config appsettings list -g rg-stockci -n stockci-api --query "[].name" -o tsv
```

## 4. Autoriser Azure dans Atlas

Atlas refuse toute connexion provenant d'une IP qui n'est pas dans son **IP Access List**. Cette liste est propre à chaque **projet** Atlas : vérifier d'être dans le projet qui contient le cluster.

Lister les IP sortantes possibles de l'App Service :

```bash
az webapp show -g rg-stockci -n stockci-api --query possibleOutboundIpAddresses -o tsv
```

Puis dans Atlas, *Security → Network Access → Add IP Address* :

- **Option sécurisée** : ajouter chacune des IP listées (une entrée par IP, notation `x.x.x.x/32`).
- **Option simple** (projet perso) : saisir `0.0.0.0/0` dans *Access List Entry*. Le cluster est alors joignable depuis n'importe quelle IP, et seul le couple utilisateur / mot de passe le protège.

Ne pas cocher *« This entry is temporary »*, et attendre que le statut passe de *Pending* à **Active**.

Une IP de réseau local (`192.168.x.x`) n'a aucun effet : Atlas voit uniquement l'IP **publique**.

## 5. Déployer

Depuis le dossier `api-stockci` :

```bash
dotnet publish -c Release -o bin/publish
(cd bin/publish && zip -r ../app.zip .)
az webapp deploy -g rg-stockci -n stockci-api --src-path bin/app.zip --type zip --async true
```

- Le zip doit contenir les fichiers **à la racine** (`api-stockci.dll`, etc.), pas un sous-dossier `publish/`.
- Publier dans `bin/` évite que le dossier de sortie soit embarqué dans les builds suivants, et `bin/` est déjà ignoré par git.
- `--async true` évite que la CLI reste bloquée sur *« Starting the site... »* alors que le site a déjà démarré.

Redémarrer après une modification des App settings ou de l'IP Access List :

```bash
az webapp restart -g rg-stockci -n stockci-api
```

## 6. Vérifier

```bash
az webapp show -g rg-stockci -n stockci-api --query state -o tsv   # → Running
```

Tester un endpoint :

- https://stockci-api.azurewebsites.net/api/Categories
- https://stockci-api.azurewebsites.net/api/Products

Une réponse JSON (même `[]`) confirme que l'API est connectée à MongoDB.

Un **404 sur `/`** ou sur `/swagger` est normal : toutes les routes sont sous `/api/...`, et Swagger n'est activé qu'en `Development`.

---

## Dépannage

### Consulter les logs

```bash
# Activer les logs (une seule fois)
az webapp log config -g rg-stockci -n stockci-api \
  --docker-container-logging filesystem --application-logging filesystem --level information

# Suivre en direct
az webapp log tail -g rg-stockci -n stockci-api

# Télécharger l'historique
az webapp log download -g rg-stockci -n stockci-api --log-file logs.zip
```

Les exceptions de l'application se trouvent dans `LogFiles/*_default_docker.log` et `LogFiles/StartupLogs/*.log`.

### Erreurs fréquentes

| Symptôme (logs) | Cause | Solution |
|---|---|---|
| `DnsResponseException ... IN TXT ... timed out` | Le réseau bloque les requêtes DNS TXT nécessaires à `mongodb+srv://` | Utiliser le format `mongodb://` avec hôtes explicites ([étape 1](#format-de-la-connection-string)) |
| `TimeoutException ... Servers : []` | Le driver n'a obtenu aucun hôte : échec DNS ou nom de cluster incorrect | Vérifier la connection string, passer au format `mongodb://` |
| `SSL Handshake failed ... tlsv1 alert internal error` | L'IP n'est pas autorisée dans Atlas | Ajouter les IP Azure ou `0.0.0.0/0` ([étape 4](#4-autoriser-azure-dans-atlas)) |
| `Authentication failed` (sans erreur SSL) | Utilisateur ou mot de passe incorrect, ou caractères spéciaux non encodés | Vérifier les identifiants dans *Database Access* |
| `StockManagementDatabase__... manquant` | Variable d'environnement absente ou mal nommée | Revoir l'[étape 3](#3-configurer-les-variables-denvironnement) |
| HTTP **500** dans le navigateur | Exception dans l'application (souvent MongoDB) | Lire les logs pour identifier l'une des causes ci-dessus |
| HTTP **502** | Requête trop longue (timeouts MongoDB) ou application en cours de redémarrage | Attendre la fin du redémarrage, puis lire les logs |
| CLI bloquée sur *« Starting the site... »* | Suivi de la CLI, le site est souvent déjà démarré | Vérifier avec `az webapp show --query state`, utiliser `--async true` |
