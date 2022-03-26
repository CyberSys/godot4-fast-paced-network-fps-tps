namespace Framework
{
    /// <summary>
    /// Required interface for service classes
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Update the service
        /// </summary>
        /// <param name="delta">Delta time of none-physics processs</param>
        public void Update(float delta);

        /// <summary>
        /// Update the service physics
        /// </summary>
        /// <param name="delta">Delta time of physics processs</param>
        public void Render(float delta);

        /// <summary>
        /// Register the service
        /// </summary>
        public void Register();

        /// <summary>
        /// Unregister the service
        /// </summary>
        public void Unregister();
    }
}
