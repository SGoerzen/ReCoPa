namespace ReCoPa.ViewModels;

public sealed class ContributorViewModel
{
    public string Name { get; }
    public string Url { get; }
    public string Email { get; }

    public ContributorViewModel(string name, string url, string email)
    {
        Name = name;
        Url = url;
        Email = email;
    }
}