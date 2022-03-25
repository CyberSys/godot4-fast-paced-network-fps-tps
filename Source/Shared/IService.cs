namespace Shooter.Shared
{
    public interface IService
    {
        public void Update(float delta);
        public void Render(float delta);
        public void Register();
        public void Unregister();
    }
}
