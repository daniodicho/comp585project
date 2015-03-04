/* Arnold Santos
 * Cesar Zalzalah
 * Dani Odicho
 * Ernie Ledezma
 */
/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file Terrain.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
    XNA 4 refresh to MonoGames 3.2.  

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
    public class Treasures : Model3D
    {
        private NavNode node = null; // SW embedded NavNode within the treasure model object
        private bool tag; // SW If this is a treasure, found or not

        // Constructor
        public Treasures(Stage theStage, string label, string fileOfModel, int x, int z)
            : base (theStage, label, fileOfModel) 
        {
            int spacing = stage.Spacing; // sets nav point
            node = new NavNode(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing),
                NavNode.NavNodeEnum.TREASURE);

            addObject(new Vector3(x * spacing, stage.Terrain.surfaceHeight(x, z), z * spacing),
                new Vector3(0, 1, 0), 0.79f); // adds treasure object to the screen
        }

        // Methods
        public NavNode Node // SW used extract NavNode from a Treasure
        {
            get { return node; }
            set { node = value; }
        }

        public bool Tag // SW used to set treasure found true or false
        {
            get { return tag; }
            set { tag = value; }
        }

        public void ChangeImg(string label) // SW Load a new mesh for the treasure model
        {
            model = stage.Content.Load<Model>(label);
        }
    }
}