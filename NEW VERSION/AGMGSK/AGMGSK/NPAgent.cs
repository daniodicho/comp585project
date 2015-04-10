/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file NPAgent.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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

namespace AGMGSKv6 {

/// <summary>
/// A non-playing character that moves.  Override the inherited Update(GameTime)
/// to implement a movement (strategy?) algorithm.
/// Distribution NPAgent moves along an "exploration" path that is created by the
/// from int[,] pathNode array.  The exploration path is traversed in a reverse path loop.
/// Paths can also be specified in text files of Vector3 values, see alternate
/// Path class constructors.
/// 
/// 12/31/2014 last changed
/// </summary>
public class NPAgent : Agent {
   private NavNode nextGoal;
   private Path path;
   private int snapDistance = 20;  // this should be a function of step and stepSize
   protected List<Treasures> TreasureList = null; // SW treasure list

	// If using makePath(int[,]) set WayPoint (x, z) vertex positions in the following array
	private int[,] pathNode = { {505, 490}, {500, 500}, {490, 505},  // bottom, right
										 {435, 505}, {425, 500}, {420, 490},  // bottom, middle
										 {420, 450}, {425, 440}, {435, 435},  // middle, middle
                               {490, 435}, {500, 430}, {505, 420},  // middle, right
										 {505, 105}, {500,  95}, {490,  90},  // top, right
                               {110,  90}, {100,  95}, { 95, 105},  // top, left
										 { 95, 480}, {100, 490}, {110, 495},  // bottom, left
										 {495, 480} };								  // loop return

   /// <summary>
   /// Create a NPC. 
   /// AGXNASK distribution has npAgent move following a Path.
   /// </summary>
   /// <param name="theStage"> the world</param>
   /// <param name="label"> name of </param>
   /// <param name="pos"> initial position </param>
   /// <param name="orientAxis"> initial rotation axis</param>
   /// <param name="radians"> initial rotation</param>
   /// <param name="meshFile"> Direct X *.x Model in Contents directory </param>
   public NPAgent(Stage theStage, string label, Vector3 pos, Vector3 orientAxis,
      float radians, string meshFile, List<Treasures> treasureList)
      : base(theStage, label, pos, orientAxis, radians, meshFile)
      {  // change names for on-screen display of current camera
      first.Name =  "npFirst";
      follow.Name = "npFollow";
      above.Name =  "npAbove";
      // path is built to work on specific terrain, make from int[x,z] array pathNode
      path = new Path(stage, pathNode, Path.PathType.LOOP); // continuous search path
      stage.Components.Add(path);
      nextGoal = path.NextNode;  // get first path goal
      agentObject.turnToFace(nextGoal.Translation);  // orient towards the first path goal
		// set snapDistance to be a little larger than step * stepSize
		snapDistance = (int) (1.5 * (agentObject.Step * agentObject.StepSize));

        TreasureList = treasureList; // needed to transfer path

      }   

   /// <summary>
   /// Simple path following.  If within "snap distance" of a the nextGoal (a NavNode) 
   /// move to the NavNode, get a new nextGoal, turnToFace() that goal.  Otherwise 
   /// continue making steps towards the nextGoal.
   /// </summary>
   public override void Update(GameTime gameTime) {
       if (stage.pathMode)
       {
           agentObject.turnToFace(nextGoal.Translation);  // adjust to face nextGoal every move
           // See if at or close to nextGoal, distance measured in 2D xz plane
           float distance = Vector3.Distance(
               new Vector3(nextGoal.Translation.X, 0, nextGoal.Translation.Z),
               new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
           stage.setInfo(15,
              string.Format("npAvatar:  location ({0:f0}, {1:f0}, {2:f0})  looking at ({3:f2}, {4:f2}, {5:f2})",
                 agentObject.Translation.X, agentObject.Translation.Y, agentObject.Translation.Z,
                 agentObject.Forward.X, agentObject.Forward.Y, agentObject.Forward.Z));
           stage.setInfo(16,
                 string.Format("npAvatar:  nextGoal ({0:f0}, {1:f0}, {2:f0})  distance to next goal = {3,5:f2})",
                     nextGoal.Translation.X, nextGoal.Translation.Y, nextGoal.Translation.Z, distance));
           if (distance <= snapDistance)
           {
               // snap to nextGoal and orient toward the new nextGoal 
               nextGoal = path.NextNode;
               // agentObject.turnToFace(nextGoal.Translation);
           }
       }
           else // SW treasure mode
            {
                bool turn = false; // SW when previous treasure has been tagged, necessary to keep track of
                // this until the next iteration to notify the agent to turn towards the next untagged treasure
                if (!nextUntagged()) // if no more treasures, return to pathfinding mode
                {
                    stage.pathMode = true;
                    agentObject.turnToFace(nextGoal.Translation);
                }
                foreach (Treasures t in TreasureList) // SW check to see if the NPAgent is close to any treasure
                {
                    if (!t.Tag) // only examine treasure if it is not tagged
                    {
                        NavNode nav = t.Node; // extract NavNode from Treasure Class
                        if (turn) // if last treasure was found, turn to the next untagged treasure
                        {
                            agentObject.turnToFace(nav.Translation);
                            turn = false;
                        }
                        float distance = Vector3.Distance(
                           new Vector3(nav.Translation.X, 0, nav.Translation.Z),
                           new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
                        if (distance <= snapDistance)
                        {
                            turn = true; // Turn to next treasure on next pass
                            IncTreasures++; // increment number of treasures that the NP agent has found
                            changePath();
                            t.Tag = true; // set treasure as found
                            String dir = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
                            playSound(dir+"\\noTreasure.wav");
                            t.Update(gameTime);
                        }
                    }
                }
            }
            base.Update(gameTime);  // Agent's Update();
       }
       private void playSound(string path)
        {
            System.Media.SoundPlayer player =
                new System.Media.SoundPlayer();
            player.SoundLocation = path;
            player.Load();
            player.Play();
        }

   public void changePath(){
       stage.pathMode = !stage.pathMode; // swap boolean for mode
       if (stage.pathMode) // if in pathfinding mode
       {
           agentObject.turnToFace(nextGoal.Translation);  // orient towards the next goal
       }
       else // if in treasure mode
       {
           if (!nextUntagged()) // if no more treasures, return to pathfinding mode
           {
               stage.pathMode = true; // Go into pathfinding mode 
               agentObject.turnToFace(nextGoal.Translation);  // orient towards the next goal
           }
       }
   }

   private bool nextUntagged() // SW turns NPAgent towards closest untagged treasure, false if no untagged treasures exist
   {
       Treasures closestTreasure = null;
       float distance, closest = float.MaxValue;

       foreach (Treasures t in TreasureList)
       {
           NavNode nav = t.Node;
           distance = Vector3.Distance(
              new Vector3(nav.Translation.X, 0, nav.Translation.Z),
              new Vector3(agentObject.Translation.X, 0, agentObject.Translation.Z));
           if (!t.Tag && distance < closest)
           {
               closest = distance;
               closestTreasure = t;
           }
       }
       if (closest == float.MaxValue)
       {
           return false; // SW no untagged treasures left
       }
       else
       {
           agentObject.turnToFace(closestTreasure.Node.Translation);
           return true;
       }
   }
   } 
}