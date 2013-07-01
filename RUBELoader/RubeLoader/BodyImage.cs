using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RubeLoader
{
    public class BodyImage
    {

        public BodyImage()
        {
            Corners = new List<Vector2>();
            Opacity = 1;
            Scale = 1;
        }

        public Body Body { get; set; }
        public Vector2 Center { get; set; }
        public List<Vector2> Corners { get; set; }
        public Texture2D Texture { get; set; }
        public string Name { get; set; }
        public float Opacity { get; set; }
        public float Scale { get; set; }
    }
}
