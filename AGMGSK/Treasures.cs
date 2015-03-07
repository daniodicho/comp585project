/* Arnold Santos   <arnold2020@yahoo.com>
 * Cesar Zalzalah  <7701707@gmail.com>
 * Dani Odicho     <dannykaka2009@hotmail.com>
 * Ernie Ledezma   <eledezma518@gmail.com>
/*  
    Copyright (C) 2015 G. Michael Barnes

    AGMGSKv6 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#if MONOGAMES //  true, build for MonoGames
using Microsoft.Xna.Framework.Storage;
#endif
#endregion

namespace AGMGSKv6
{
    public class Treasures : MovableModel3D
    {
        private NavNode node = null; // nav node needed for the treasue
        private bool tag; // indicates whether the treasure was tagged

        // Constructor
        public Treasures(Stage theStage, string label, string fileOfModel, int x, int z)
            : base (theStage, label, fileOfModel) 
        {
            int spacing = stage.Spacing; // sets the spacing of nav point
            node = new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing),
                NavNode.NavNodeEnum.TREASURE);

            addObject(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing),
                new Vector3(0, 1, 0), 0.75f); // add treasure object to the above the terrain

        }

        public NavNode Node // SW used extract NavNode from a Treasure
        {
            get { return node; }
            set { node = value; }
        }

        public bool Tag //set tage to true when tagged
        {
            get { return tag; }
            set { tag = value; }
        }

        //Moves the treasure to the other end of the terrain
        public override void Update(GameTime gameTime)
        {
            if (tag)
            {
                foreach (Object3D obj in instance)
                {
                    obj.Step = 0;
                    obj.Step+=5;
                    obj.updateMovableObject();
                }
                base.Update(gameTime);
            }
        }
    }
}