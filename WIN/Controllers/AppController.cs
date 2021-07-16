using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using EasyEncryption;
using App.Models;
using create.db;


namespace App.Controllers
{

    public class AppController : ControllerBase
    {
        // API Route GET: /app/
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("/app")]
        public ActionResult<IEnumerable<app>> getapp()
        {
            using (var context = new DatabaseContext())
            {
                var app = context.App.ToList();
                if (app.Count == 0)
                    return BadRequest(new { message = "Il n'y a pas d'app dans la base de données" });
                return app;
            }
        }

        // API Route GET: /app/{id}
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("/app/{id}")]
        public ActionResult<IEnumerable<app>> GetAppById(int id)
        {
            using (var context = new DatabaseContext())
            {
                var appid = context.App.Find(id);
                if (appid == null)
                    return BadRequest(new { message = "Aucune app correspond à cet id dans la base de données" });
                return Ok(appid);
            }
        }

        // API Route POST: /app/create
        [AllowAnonymous]
        [HttpPost("/app/create")]
        public object createapp([FromBody] app us)
        {
            var context = new DatabaseContext();
            app _app = new app { username = us.username, name = us.name, email = us.email, password = us.password };

            var use = context.App.Where(us => us.email == us.email).FirstOrDefault();
            if (String.IsNullOrEmpty(_app.email) == true || IsValidEmail(_app.email) == false || String.IsNullOrEmpty(_app.password) == true || String.IsNullOrEmpty(_app.username) == true || String.IsNullOrEmpty(_app.name) == true)
            {
                return BadRequest(new { message = "Les informations saisies sont erronnées, l'app n'a pas été créee." });
            }
            else if (use != null)
            {
                return BadRequest(new { message = "L'adresse mail à déjà été utilisée" });
            }
            else
            {
                _app.password = SHA.ComputeSHA256Hash(_app.password);
                var add = context.Set<app>();
                add.Add(_app);
                context.SaveChanges();
                return StatusCode(201, new { message = "L'app est créee" });
            }
        }


       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("app/update/{id}")]
        public object Put(int id, [FromBody] app app)
        {
            using (DatabaseContext dbContext = new DatabaseContext())
            {
                var upd = dbContext.App.Find(id);
                if (upd == null)
                    return BadRequest(new { message = "L'app n'a pas pu être mis à jour car elle n'existe pas" });
                var entity = dbContext.App.FirstOrDefault(e => e.id == id);
                if (String.IsNullOrEmpty(app.username) == true || String.IsNullOrEmpty(app.name) == true || String.IsNullOrEmpty(app.email) == true || IsValidEmail(app.email) == false || String.IsNullOrEmpty(app.password) == true)
                {
                    return BadRequest(new { message = "Vous avez oublié des champs ou bien les informations saisies ne sont pas correctes" });
                }
                else
                {
                    entity.username = app.username;
                    entity.name = app.name;
                    entity.email = app.email;
                    entity.password = SHA.ComputeSHA256Hash(app.password);
                    dbContext.SaveChanges();
                    return (entity);
                }
            }
        }

        // API Route DELETE: /app/delete/{id}
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("/app/delete/{id}")]
        public ActionResult<IEnumerable<app>> delete(int id)
        {
            using (var context = new DatabaseContext())
            {
                var delete = context.App.Find(id);
                if (delete == null)
                    return BadRequest(new { message = "L'app n'a pas pu être supprimé car elle n'existe pas" });
                context.App.Remove(delete);
                context.SaveChanges();
                return Ok(new { message = "L'app correspondant à l'id " + id + " à été supprimé" });
            }

        }

        // API Route POST: /app/authentificate
        [AllowAnonymous]
        [HttpPost("/app/authentificate")]
        public IActionResult authenticate([FromBody] app ap)
        {
            var context = new DatabaseContext();
            app us = new app { username = ap.username, name = ap.name, email = ap.email, password = ap.password };
            try
            {
                var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ"));
                var now = context.App.FirstOrDefault(u => u.email == ap.email);
                if (SHA.ComputeSHA256Hash(ap.password) != now.password)
                    return BadRequest(new { message = "Le mot de passe n'est pas valide" });

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Expires = DateTime.UtcNow.AddHours(6),
                    SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Ok(new
                {
                    token = tokenHandler.WriteToken(token)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = "L'app n'existe pas" });
            }
        }

        bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z");
        }
    }
}


