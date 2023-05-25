namespace dotnet_project.Services
{
    public interface ICharacterService
    {
         int _count { get; set; }
        int Add(int count);
        int Get();
    }
}