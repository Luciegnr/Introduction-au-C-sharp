
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Text;
using System;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using EasyEncryption;
using User.Models;
using create.db;


namespace Users.Controllers
{

    public class UserController : ControllerBase
    {


        // Création d'un user
        [AllowAnonymous]
        [HttpPost]
        [Route("/users/create")]
        public object create([FromBody] users us)
        {
            var context = new DatabaseContext();
            users _us = new users { email = us.email, password = us.password, role = us.role };

            if (String.IsNullOrEmpty(_us.role) == true)
                _us.role = role.utilisateur;

            var use = context.Users.Where(u => u.email == us.email).FirstOrDefault();
            if (String.IsNullOrEmpty(_us.email) == true || IsValidEmail(_us.email) == false || String.IsNullOrEmpty(_us.password) == true || _us.role != "admin" && _us.role != "utilisateur")
            {
                return BadRequest(new { message = "Les informations saisies sont erronnées, l'utilisateur n'a pas été crée." });
            }
            else if (use != null)
            {
                return BadRequest(new { message = "L'adresse mail à déjà été utilisée" });
            }
            else
            {
                _us.password = SHA.ComputeSHA256Hash(_us.password);
                var add = context.Set<users>();
                add.Add(_us);
                context.SaveChanges();
                return StatusCode(201, new { message = "L'utilisateur " + _us.email + " avec le rôle d'" + _us.role + " à été crée" });
            }
        }

        // Système d'authentification
        [AllowAnonymous]
        [HttpPost("/users/authentificate")]
        public IActionResult authenticate([FromBody] users user)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                users us = new users { email = user.email, password = user.password, role = user.role };
                try
                {
                    var now = context.Users.FirstOrDefault(u => u.email == user.email);
                    if (SHA.ComputeSHA256Hash(user.password) != now.password)
                        return BadRequest(new { message = "Le mot de passe n'est pas valide" });

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ");
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    // new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, now.role)
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return Ok(new
                    {
                        token = tokenHandler.WriteToken(token)
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return BadRequest(new { message = "L'utilisateur n'existe pas" });
                }
            }
        }

        // Récupération de tous les users
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin, utilisateur")]
        [HttpGet("/users")]
        public ActionResult<IEnumerable<users>> getusers()
        {
            using (var context = new DatabaseContext())
            {
                var users = context.Users.ToList();
                if (users.Count == 0)
                    return BadRequest(new { message = "Il n'y a pas d'utilisateur dans la base de données" });
                return users;
            }
        }


        // Récupération d'un user 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin, utilisateur")]
        [HttpGet("/users/{id}")]
        public ActionResult<IEnumerable<users>> GetUserById(int id)
        {
            using (var context = new DatabaseContext())
            {
                var usid = context.Users.Find(id);
                if (usid == null)
                    return BadRequest(new { message = "Aucun utilisateur correspond à cet id dans la base de données" });
                return Ok(usid);
            }
        }


        // Système pour mettre à jour les informations d'un user
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin, utilisateur")]
        [HttpPut("users/update/{id}")]
        public object Put(int id, [FromBody] users us)
        {
            using (DatabaseContext dbContext = new DatabaseContext())
            {
                var upd = dbContext.Users.Find(id);
                if (upd == null)
                    return BadRequest(new { message = "L'utilisateur n'a pas pu être mis à jour car il n'existe pas" });
                var entity = dbContext.Users.FirstOrDefault(e => e.id == id);
                if (String.IsNullOrEmpty(us.email) == true || IsValidEmail(us.email) == false || String.IsNullOrEmpty(us.password) == true)
                {
                    return BadRequest(new { message = "Vous avez oublié des champs ou bien les informations saisies ne sont pas correctes" });

                }
                else
                {
                    if (us.role == "admin" || us.role == "utilisateur")
                    {
                        entity.role = us.role;
                    }
                    entity.email = us.email;
                    entity.password = SHA.ComputeSHA256Hash(us.password);
                    dbContext.SaveChanges();
                    return (entity);
                }
            }
        }

        // Suppression d'un user en particulier
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpDelete("/users/delete/{id}")]
        public ActionResult<IEnumerable<users>> delete(int id)
        {
            using (var context = new DatabaseContext())
            {
                var del = context.Users.Find(id);
                if (del == null)
                    return BadRequest(new { message = "L'utilisateur n'a pas pu être supprimé car il n'existe pas" });
                context.Users.Remove(del);
                context.SaveChanges();
                return Ok(new { message = "L'utilisateur correspondant à l'id " + id + " à été supprimé" });
            }
        }

        bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z");
        }
    }
}


