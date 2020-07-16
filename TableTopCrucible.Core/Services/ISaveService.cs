namespace TableTopCrucible.Core.Services
{
    public interface ISaveService
    {
        void Load(string file);
        void Save(string file);
    }
}
