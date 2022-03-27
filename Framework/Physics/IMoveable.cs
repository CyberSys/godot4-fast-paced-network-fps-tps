namespace Framework.Physics
{

    public interface IMoveable : IChildComponent
    {
        public void setCrouchingLevel(float level);
        public void activateColliderShape(bool isColiderActive);
        public Godot.Vector3 Velocity { get; set; }
        public Godot.Transform3D Transform { get; set; }
        public void Move(float delta, Godot.Vector3 velocity);
        public bool isOnGround();
    }
}
