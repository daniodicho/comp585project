/* Arnold Santos
 * Cesar Zalzalah
 * Dani Odicho
 * Ernie Ledezma
  
    Copyright (C) 2015 G. Michael Barnes
 
    The file Player.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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
#if ! __XNA4__  // when __XNA4__ == true build for MonoGames
   using Microsoft.Xna.Framework.Storage; 
#endif
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace AGMGSKv6
{

    /// <summary>
    /// Represents the user / player interacting with the stage. 
    /// The Update(Gametime) handles both user keyboard and gamepad controller input.
    /// If there is a gamepad attached the keyboard inputs are not processed.
    /// 
    /// removed game controller code from Update()
    /// 
    /// 2/8/2014 last changed
    /// </summary>

    public class Player : Agent
    {
        //private GamePadState oldGamePadState;
        private KeyboardState oldKeyboardState;
        private int rotate;
        private float angle;
        private Matrix initialOrientation;
        private int tagDistance = 500; // SW Distance to detect player tagging a treasure
        protected List<Treasures> TreasureList = null; // SW stores passed list of treasures from stage

        public Player(Stage theStage, string label, Vector3 pos, Vector3 orientAxis,
        float radians, string meshFile, List<Treasures> treasureList)
            : base(theStage, label, pos, orientAxis, radians, meshFile)
        {  // change names for on-screen display of current camera
            first.Name = "First";
            follow.Name = "Follow";
            above.Name = "Above";
            IsCollidable = true;  // players test collision with Collidable set.
            stage.Collidable.Add(agentObject);  // player's agentObject can be collided with by others.
            rotate = 0;
            angle = 0.01f;
            initialOrientation = agentObject.Orientation;

            TreasureList = treasureList; // SW ** Necessary to transfer path
        }

        /// <summary>
        /// Handle player input that affects the player.
        /// See Stage.Update(...) for handling user input that affects
        /// how the stage is rendered.
        /// First check if gamepad is connected, if true use gamepad
        /// otherwise assume and use keyboard.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.R) && !oldKeyboardState.IsKeyDown(Keys.R))
                agentObject.Orientation = initialOrientation;
            // allow more than one keyboardState to be pressed
            if (keyboardState.IsKeyDown(Keys.Up)) agentObject.Step++;
            if (keyboardState.IsKeyDown(Keys.Down)) agentObject.Step--;
            if (keyboardState.IsKeyDown(Keys.Left)) rotate++;
            if (keyboardState.IsKeyDown(Keys.Right)) rotate--;
            oldKeyboardState = keyboardState;    // Update saved state.
            agentObject.Yaw = rotate * angle;
            base.Update(gameTime);
            rotate = agentObject.Step = 0;

            // SW player treasure tagging
            foreach (Treasures t in TreasureList)
            {
                NavNode nav = t.Node; // extract NavNode from treasure object
                float distance = Vector3.Distance(
                    new Vector3(nav.Translation.X, 0, nav.Translation.Z),
                    new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                if (distance <= tagDistance && t.Tag == false)
                {
                    IncTreasures++; // increment number of treasures that the agent has found
                    t.Tag = true; // set treasure as found 
                    t.IsCollidable = false;
                    String dir = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
                    playSound(dir + "\\noTreasure.wav");
                    t.Update(gameTime);
                }
            }
        }
        private void playSound(string path)
        {
            System.Media.SoundPlayer player =
                new System.Media.SoundPlayer();
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }
    }

}