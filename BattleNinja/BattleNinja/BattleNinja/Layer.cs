using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BattleNinja
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Layer
    {
        public Texture2D[] Textures { get; private set; }
        public float ScrollRate { get; private set; }

        public Layer(ContentManager content,string basePath,float scrollRate)
        {
            Textures = new Texture2D[3];
            for (int i = 0; i < 3; ++i)
            {
                Textures[i] = content.Load<Texture2D>(basePath + "_" + i);
            }
            ScrollRate = scrollRate;
        }

        public void Draw(SpriteBatch spriteBatch, float cameraPosition)
        {
            int segmentWidth = Textures[0].Width;

            float x = cameraPosition*ScrollRate;
            int leftSegment = (int) Math.Floor(x/segmentWidth);
            int rightSegment = leftSegment + 1;
            x = (x/segmentWidth - leftSegment)*-segmentWidth;

            spriteBatch.Draw(Textures[leftSegment% Textures.Length], new Vector2(x,0.0f),Color.White );
            spriteBatch.Draw(Textures[rightSegment % Textures.Length],new Vector2(x + segmentWidth, 0.0f), Color.White);
        }
    }
}
