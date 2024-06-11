using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RazorPagesLab3.Models;

namespace RazorPagesLab3.Pages
{
    [IgnoreAntiforgeryToken]
    public class contactModel : PageModel
    {

        private EmailAddressAttribute validator = new EmailAddressAttribute();
        private readonly ContactsWriter cw;


        private ApplicationDbContext applicationDbContext;

        public contactModel(ContactsWriter _cw, ApplicationDbContext _applicationDbContext)
        {
            this.cw = _cw;
            this.applicationDbContext = _applicationDbContext;
        }

        public void OnGet() {}


        public IActionResult OnPost()
        {

            if (Request.Form["first_name"] == string.Empty)
                return Content("<div class=\"error_message\">Attention! You must enter your name.</div>");
            if (Request.Form["email"] == string.Empty)
                return Content("<div class=\"error_message\">Attention! Please enter a valid email address.</div>");
            if (!validator.IsValid(Request.Form["email"].ToString()))
                return Content("<div class=\"error_message\">Attention! You have enter an invalid e-mail address, try again.</div>");
            if (Request.Form["comments"] == string.Empty)
                return Content("<div class=\"error_message\">Attention! Please enter your message.</div>");

            ContactForm contact = new ContactForm
            {
                FirstName = Request.Form["first_name"],
                LastName = Request.Form["last_name"],
                Email = Request.Form["email"],
                Phone = Request.Form["phone"],
                SelectService = Request.Form["select_service"],
                SelectPrice = Request.Form["select_price"],
                Comments = Request.Form["comments"]
            };
            cw.WriteInCSV(contact);

            Response.StatusCode = 200;

            applicationDbContext.Database.EnsureCreated();
            applicationDbContext.Database.Migrate();
            applicationDbContext.Contacts.Add(contact);
            applicationDbContext.SaveChanges();


            return Content($@"<fieldset>
                              <div id='success_page'>
                              <h1>Email Sent Successfully.</h1>
                              <p>Thank you <strong>{Request.Form["first_name"]}</strong>, your message has been submitted to us.</p>
                              </div>
                              </fieldset>");
        }

        
    }
}
