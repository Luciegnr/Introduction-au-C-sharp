# TIC-WIN1 / Introduction au C#
## Etape 1

Tout d'abord nous devons créer un projet avec cette commande (ici notre projet se nomme WIN) : `dotnet new mvc -o WIN` 

Pour que le projet se lance sur le port 4242 nous devons aller dans :
* `Properties` puis dans le fichier `laucnhSettings.json` pour changer la ligne suivante comme ceci :`applicationUrl": "https://localhost:5001;http://localhost:4242`

Pour que le projet ne soit pas en HTTPS nous commentons dans le fichier `Startup.cs` la ligne :

> app.UseHttpsRedirection(); 

Pour la création de la route `/home/hello` nous avons crée un Controller `HelloWordController` nous y avons ajouter une fonction `HelloWord()` pour qu'elle renvoie en Json la phrase `"Hello World"`
```
namespace WIN.Controllers
{
    public class HelloWordController : Controller
    {
        public IActionResult HelloWord()
        {
            return Json(new { etna = "Hello World"});
        }
    }
}
```
Puis nous appelons cette fonction dans le fichier `Startup.cs` au niveau de la ligne `app.UseEndpoints(endpoints =>`
```
endpoints.MapControllerRoute(name: "HelloWord",
    pattern: "home/hello",
    defaults: new { controller = "HelloWord", action = "HelloWord" });
```

Pour constuire notre projet `dotnet build`
Pour finir pour lancer le projet nous faisont `dotnet run`

## Etape 2 

Pour créer notre base de données nous avons ajouté dans un dossier `Database` un fichier `dbconfig.cs` avec dedans une classe DbContext afin de nous y connecter.
```
namespace create.db
{
    public class DatabaseContext : DbContext
    {
        public DbSet<users> Users { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=database.db");
            // => options.UseSqlite(@"Data Source=/Users/lucie/Desktop/ETNA/TIC/WIN1/WIN/database.db");

        }
    
}
```

Ensuite nous avons crée une table `Users` avec les champs `Email, ID, Password, Rôle`.

```
namespace User.Models
{
        public class users
    {        
        public string email { get; set; }      
        public int id { get; set; }
        public string password { get; set; }
        public string role { get; set; }
    }

}
```

## Etape 3

Nous devons mettre en place la migration de notre table `Users` pour cela nous faisons les commandes suivantes:

* `dotnet ef database update 0`
* `dotnet ef migrations remove` 
* `dotnet ef migrations add Init`
* `dotnet ef database update`

Ne pas oublier d'installer `dotnet tool install --global dotnet-ef` afin d'utiliser les commandes.

## Etape 4

Nous avons mis en place notre `UsersController` pour permettre à l'utilisateur d'effectuer plusieurs actions. Nous avons donc mis en place les routes suivantes :

* /users/ ⇒ récupérer la liste de vos utilisateurs (GET).
* /users/id ⇒ récupérer les informations d'un utilisateur (GET).
* /users/create ⇒ créer un utilisateur (POST).
* /users/update/{id} ⇒ modifier un utilisateur (PUT).
* /users/delete/{id} ⇒ supprimer un utilisateur (DELETE).

Pour chaque routes nous avons crée une fonction.
Nous avons mis en place lors de la création d'un utilisateur que son mot de passe soit haché via l'algorithme SHA256 ainsi qu'une vérification du champ email qu'il soit correctement rempli de la forme ` ***@***.com` . 

Vérification de la forme de l'email 
```
bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z");
    }
```

## Etape 5

Création d'une route `/users/authentificate` dans le `UsersController` pour pouvoir authentifier nos utilisateurs. Puis notre route prendra en format JSON les champs email et password pour ensuite renvoyer un token JWT.
Pour cela nous avons mis dans la route `/users/authentificate` une fonction qui permet de génerer le token comme ceci :
![](https://i.imgur.com/9XQXeix.png)

## Etape 6

Nous devons ici mettre notre token dans une "clé secrete" afin de pouvoir le stocker, A partir de la ligne 131 dans `UsersController`

            
Nous devons aussi ajouter dans `Startup.cs` les lignes suivantes :

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ"))
        };
    });

Ensuite nous devons ajouter une vérification de route pour que seul les utilisateurs authentifiés est accès aux différentes routes. Pour cela nous ajoutons devant chaque route dans le `UsersController` ceci `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` qui signifie qu'il demande une autorisation.


## Etape 7

Nous avons commencé la mise en place d'un middleware pour que la route /users/delete ne soit accessible que pour les utilisateurs possédant le role `admin` et pour que toutes les autres routes soient accessibles uniquement si l'utilisateur est authentifier (sauf les routes create et authentificate).

Pour ce faire, nous avons passé le rôle de notre user dans la génération de son Token:

```
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new Claim[]
    {
        new Claim(ClaimTypes.Role, now.role)
    }),
    Expires = DateTime.UtcNow.AddDays(7),
    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
```
Et nous, précisons dans notre middleware que nous voulons que l'utilisateur ait le role "admin" pour pouvoir supprimer un utilisateur:

`[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]`

