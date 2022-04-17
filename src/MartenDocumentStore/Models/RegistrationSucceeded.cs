namespace MartenDocumentStore.Models;

public class RegistrationSucceeded
{
  public RegistrationSucceeded(Registration registration, Guid id)
  {
    FirstName = registration.FirstName;
    Id = id.ToString();
  }
  public string FirstName { get; set; }
  public string Id { get; set; }
}
