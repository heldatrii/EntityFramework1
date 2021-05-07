using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Base;
using UserManagement.Context;
using UserManagement.Models;
using UserManagement.Repository.Data;
using UserManagement.ViewModel;

namespace UserManagement.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class AccountsController : BaseController<Account, AccountRepository, string>
    {
        private readonly AccountRepository accountRepository;
        private readonly MyContext myContext;
        public IConfiguration _configuration;


        public AccountsController(AccountRepository accountRepository, MyContext myContext, IConfiguration configuration) : base(accountRepository)
        {
            this.accountRepository = accountRepository;
            this.myContext = myContext;
            _configuration = configuration;
        }

        //insert data from multiple table
        [HttpPost("Register")]
        public ActionResult Register(RegisterVM registerVM) 
        {
            Person person = new Person();
            person.NIK = registerVM.NIK;
            person.FirstName = registerVM.FirstName;
            person.LastName = registerVM.LastName;
            person.Phone = registerVM.Phone;
            person.BirthDate = registerVM.BirthDate;
            person.Salary = registerVM.Salary;
            person.Email = registerVM.Email;
            myContext.Persons.Add(person);
            myContext.SaveChanges();

            Account account = new Account();
            account.NIK = registerVM.NIK;
            account.Password = Hashing.HashPassword(registerVM.Password);
            myContext.Accounts.Add(account);
            myContext.SaveChanges();
           
            Education education = new Education();
            education.Degree = registerVM.Degree;
            education.GPA = registerVM.GPA;
            education.UniversityID = registerVM.UniversityID;
            myContext.Educations.Add(education);
            myContext.SaveChanges();

            Profiling profiling = new Profiling();
            profiling.NIK = registerVM.NIK;
            profiling.EducationID = education.EducationID;
            myContext.Profilings.Add(profiling);
            myContext.SaveChanges();

            //Role role = new Role();
            //role.Name = registerVM.Name;
            //myContext.Roles.Add(role);
            //myContext.SaveChanges();

            AccountRole accountRole = new AccountRole();
            accountRole.NIK = person.NIK;
            accountRole.RoleId = 1;
            myContext.AccountRoles.Add(accountRole);
            myContext.SaveChanges();
            
            return Ok();
        }

        //get all data from multiple more than 3 tables
       // [Authorize]
        [HttpGet("UserData")]
        public async Task<ActionResult> GetAll()
        {
            var  viewmodel = from p in myContext.Persons
                             join a in myContext.Accounts on p.NIK equals a.NIK
                             join f in myContext.Profilings on p.NIK equals f.NIK
                             join ar in myContext.AccountRoles on p.NIK equals ar.NIK
                             join r in myContext.Roles on ar.RoleId equals r.RoleId
                             join e in myContext.Educations on f.EducationID equals e.EducationID
                             select new
                             {
                                 NIK = p.NIK,
                                 FirstName = p.FirstName,
                                 LastName = p.LastName,
                                 Phone = p.Phone,
                                 BirthDate = p.BirthDate,
                                 Salary = p.Salary,
                                 Email = p.Email,
                                // Password = a.Password,
                                 EducationID = f.EducationID,
                                 UniversityID = e.UniversityID,
                                 GPA = e.GPA,
                                 Degree = e.Degree,
                                 RoleId = ar.RoleId,
                                 Name = r.Name
                                
                             };
            return Ok(await viewmodel.ToListAsync());
        }

      
        //get data by ID(NIK)
        [Authorize]
        [HttpGet("Profile/{NIK}")]
        public ActionResult GetById(string NIK)
        {  
            var viewmodel = (from p in myContext.Persons
                             join a in myContext.Accounts on p.NIK equals a.NIK
                             join f in myContext.Profilings on p.NIK equals f.NIK
                             join ar in myContext.AccountRoles on p.NIK equals ar.NIK
                             join r in myContext.Roles on ar.RoleId equals r.RoleId
                             join e in myContext.Educations on f.EducationID equals e.EducationID
                             where p.NIK == NIK

                             select new RegisterAllVM
                             {
                                 NIK = p.NIK,
                                 FirstName = p.FirstName,
                                 LastName = p.LastName,
                                 Phone = p.Phone,
                                 BirthDate = p.BirthDate,
                                 Salary = p.Salary,
                                 Email = p.Email,
                               //  Password = a.Password,
                                 EducationID = f.EducationID,
                                 UniversityID = e.UniversityID,
                                 GPA = e.GPA,
                                 Degree = e.Degree,
                                 RoleId = ar.RoleId,
                                 Name = r.Name

                             }).ToList();
            return Ok(viewmodel);
        }

        
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginVM loginVM) 
        {
            var person = myContext.Persons.Where(p => p.Email == loginVM.Email).FirstOrDefault();

            if (person != null)
            {
              //  var user = myContext.Accounts.Where(a => a.Password == loginVM.Password && a.NIK == person.NIK/).FirstOrDefault();
                  var user = myContext.Accounts.Where(a => a.NIK == person.NIK).FirstOrDefault();
                
             //     var roleName = myContext.Roles.Where(l => l.RoleId == role.RoleId).FirstOrDefault();
                if (user != null && Hashing.ValidatePassword(loginVM.Password, user.Password))
             //   if (account != null)
                {
                    // List<Claim> claims = new List<Claim>();
                    //  var role = myContext.AccountRoles.Where(l => l.NIK == person.NIK).FirstOrDefault() ;
                    var role = myContext.AccountRoles.Where(r => r.NIK == user.NIK).ToList();
                    var claims = new List<Claim> {
                    
                //    new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                    new Claim("NIK", person.NIK),
                    new Claim("FirstName", person.FirstName),
                    new Claim("LastName", person.LastName),
                    new Claim("Email", person.Email),
                 //  new Claim ("Roles", role.RoleId)
                   };

                    foreach (var item in role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, myContext.Roles.Where(r => r.RoleId == item.RoleId).FirstOrDefault().Name));
                    }

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"], 
                        audience :_configuration["Jwt:Audience"], 
                        claims, 
                        expires: DateTime.UtcNow.AddMinutes(10), 
                        signingCredentials: signIn);
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                   // return GetById(person.NIK);
                }
                else
                {
                    return NotFound("Error Email/Password is Invalid");
                }
            }
            else
            {
                return NotFound("Error Email/Password is Invalid");
            }
        }

        [HttpPost("ChangePassword")]
        public ActionResult updatepassword(ChangePasswordVM changePasswordVM)
        {
            var cek = myContext.Accounts.FirstOrDefault(a => a.NIK == changePasswordVM.NIK);

            if (cek != null && Hashing.ValidatePassword(changePasswordVM.CurrentPassword, cek.Password))
            {
                    cek.Password = Hashing.HashPassword(changePasswordVM.NewPassword);
                    myContext.Entry(cek).State = EntityState.Modified;
                    myContext.SaveChanges();
                    return Ok();
            }
            else { return NotFound(); }
        }

        [Authorize]
        [HttpPost("SendEmail")]
        public IActionResult SendEmail(ForgetPasswordVM forgetPasswordVM)
        {
            var resetpassword = Guid.NewGuid().ToString();

            Person person = myContext.Persons.Where(p => p.Email == forgetPasswordVM.Email).FirstOrDefault();
            if (person != null)
            {
                Account account = myContext.Accounts.Where(a => a.NIK == person.NIK).FirstOrDefault();
                
                var resultAccount = myContext.Accounts.Find(person.NIK);
                resultAccount.Password = Hashing.HashPassword(resetpassword);
                myContext.Entry(resultAccount).State = EntityState.Modified;
                myContext.SaveChanges();

                var user = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("namosayoii@gmail.com", "11ii0399i"),
                    EnableSsl = true
                };
                user.Send("namosayoii@gmail.com", forgetPasswordVM.Email, "Reset Password Request", $"Haloo {person.FirstName} how's ur day ? " +
                    $"i wish ur day always beutiful like a rainbow and here is your New Password : {resetpassword}");
                return Ok("request sent");
            }
            else
            {
                return NotFound("Email Not Found");
            }

        }
        //jkjnjnkm


    }
}