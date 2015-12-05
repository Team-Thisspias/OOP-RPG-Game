namespace BattleNinja
{
    using Microsoft.Xna.Framework;

    public struct Circle
    {
        public Circle(Vector2 position, float radius)
            :this()
        {
            this.Center = position;
            this.Radius = radius;
        }

        public Vector2 Center { get; set; }

        public float Radius { get; set; }

        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = this.Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < this.Radius * this.Radius));
        }
    }
}
