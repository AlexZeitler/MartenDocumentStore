using System.Linq;
using MartenDocumentStore.Models;
using Easy_Password_Validator;
using Easy_Password_Validator.Models;
using Marten;
using Marten.Linq;
using Microsoft.AspNetCore.Mvc;
using static MartenDocumentStore.Controllers.Constants;

namespace MartenDocumentStore.Controllers;

public class RegistrationController : Controller
{
  private readonly IDocumentStore store;

  public RegistrationController(IDocumentStore store)
  {
    this.store = store;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpPost]
  [Route("/api/registration")] //same route but different "Consumes"
  [Consumes(FormUrlEncoded)]
  public async Task<ActionResult> PostForm([FromForm] Registration registration)
  {
    if (!string.IsNullOrWhiteSpace(registration.Email))
    {
      await using var session = store.QuerySession();
      var users = await session.Query<User>()
        .Where(u => u.Email == registration.Email)
        .ToListAsync();
      if (users.Count == 1)
      {
        ModelState.AddModelError("email", $"User with Email {registration.Email} already exists.");
      }
    }

    if (!ModelState.IsValid)
    {
      return PartialView("_RegistrationForm", registration);
    }

    var id = Guid.NewGuid();

    await using (var session = store.OpenSession())
    {
      session.Store(new User()
      {
        FirstName = registration.FirstName, LastName = registration.LastName, Email = registration.Email,
        CompanyName = registration.CompanyName, Id = id
      });

      await session.SaveChangesAsync();
    }

    return PartialView("_RegistrationSucceeded", new RegistrationSucceeded(registration, id));
  }

  [HttpPost]
  [Route("/api/registration")]
  [Consumes(ApplicationJson)]
  public async Task<ActionResult> PostJson([FromBody] Registration registration)
  {
    if (!ModelState.IsValid)
    {
      var validation = new ValidationProblemDetails(ModelState)
      {
        Status = StatusCodes.Status422UnprocessableEntity
      };
      return UnprocessableEntity(validation);
    }


    return Created($"/user/{Guid.NewGuid()}", new { registration.FirstName });
  }

  [HttpPost]
  [Route("/api/passwordvalidation", Name = "passwordvalidation")]
  public IActionResult PasswordValidation([FromForm] PasswordValidation passwordValidation)
  {
    if (string.IsNullOrWhiteSpace(passwordValidation.Password))
    {
      return PartialView("_PasswordStrength", new PasswordValidation() { PasswordScore = null });
    }

    var validator = new PasswordValidatorService(new PasswordRequirements());
    validator.TestAndScore(passwordValidation.Password);
    return PartialView("_PasswordStrength", new PasswordValidation() { PasswordScore = validator.Score });
  }
}
