using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SlayerKnight
{
    internal class LevelFeature : UpdateInterface, DrawInterface, RoomInterface
    {
        public RoomFeature RoomFeatureObject { get; private set; }
        public LevelFeature(string identifier)
        {
            RoomFeatureObject = new RoomFeature()
            {
                Identifier=identifier,
                UpdateObject=this,
                DrawObject=this
            };
        }
        public void Update(float timeElapsed)
        {

        }
        public void Draw(Matrix? transformMatrix)
        {

        }
    }
}
