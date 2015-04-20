/* Arnold Santos   <arnold2020@yahoo.com>
 * Cesar Zalzalah  <7701707@gmail.com>
 * Dani Odicho     <dannykaka2009@hotmail.com>
 * Ernie Ledezma   <eledezma518@gmail.com>
 * 
 * Portions of code and ideas used from:
 * http://www.red3d.com/cwr/boids/
 * http://gamedevelopment.tutsplus.com/tutorials/the-three-simple-rules-of-flocking-behaviors-alignment-cohesion-and-separation--gamedev-3444
 * 
 * 
/*

/*
    Copyright (C) 2015 G. Michael Barnes

    The file Pack.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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

    /// <summary>
    /// Pack represents a "fvk" of MovableObject3D's Object3Ds.
    /// Usually the "player" is the leader and is set in the Stage's LoadContent().
    /// With no leader, determine a "virtual leader" from the fvk's members.
    /// Model3D's inherited List<Object3D> instance holds all members of the pack.
    ///
    /// 2/1/2015 last changed
    /// </summary>
    public class Pack : MovableModel3D
    {
        Object3D leader;
        double flockingPercent = 0.0;

        /// <summary>
        /// Construct a pack with an Object3D leader
        /// </summary>
        /// <param name="theStage"> the scene </param>
        /// <param name="label"> name of pack</param>
        /// <param name="meshFile"> model of a pack instance</param>
        /// <param name="xPos, zPos">  approximate position of the pack </param>
        /// <param name="aLeader"> alpha dog can be used for fvk center and alignment </param>
        public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
            : base(theStage, label, meshFile)
        {
            isCollidable = true;
            random = new Random();
            leader = theLeader;
            int spacing = stage.Spacing;
            // initial vertex offset of dogs around (xPos, zPos)
            int[,] position = { { 0, 0 }, { 7, -4 }, { -5, -2 }, { -7, 4 }, { 5, 2 }, { 9, 7 }, { 6, 9 }, { -9, 2 } };
            for (int i = 0; i < position.GetLength(0); i++)
            {
                int x = xPos + position[i, 0];
                int z = zPos + position[i, 1];
                float scale = (float)(0.5 + random.NextDouble());
                addObject(new Vector3(x * spacing, stage.surfaceHeight(x, z), z * spacing),
                              new Vector3(0, 1, 0), 0.0f,
                              new Vector3(scale, scale, scale));
            }
        }

        /// <summary>
        /// Each pack member's orientation matrix will be updated.
        /// Distribution has pack of dogs moving randomly.
        /// Supports leaderless and leader based "fvking"
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            foreach (Object3D obj in instance)
            {
                float distance;

                if (random.NextDouble() * 100 < flockingPercent) // if flocking is present there must be leader
                {

                    if (leader == null)
                    {
                        distance = Vector3.Distance(obj.Translation, getCenter(obj)); //checks distance between aliens

                    }
                    else
                    {
                        distance = Vector3.Distance(obj.Translation, leader.Translation); //checks distance between alien and leader
                    }

                    float innerDistance = 500f;
                    float middleDistance = 900f;
                    float totalDistance = innerDistance + middleDistance;

                    if (distance < innerDistance) // if close to leader move around more
                    {
                        computeSeparation(obj);
                    }
                    else if (distance < middleDistance) // if in the midle start separating a bit more, and moving around
                    {
                        if (random.NextDouble() < (distance - innerDistance) / (middleDistance - innerDistance))
                            computeSeparation(obj);
                        else
                            computeAlignment(obj);
                    }
                    else if (distance < totalDistance) // makes sure that aliens dont get stuck with eachother.
                    {

                        computeAlignment(obj);
                    }

                    else
                        computeCohesion(obj);
                }
                else //if no leader do normal behavior
                {
                    float angle = 0.3f;
                    obj.Yaw = 0.0f;
                    // change direction 4 time a second  0.07 = 4/60
                    if (random.NextDouble() < 0.07)
                    {
                        if (random.NextDouble() < 0.5) obj.Yaw -= angle; // turn left
                        else obj.Yaw += angle; // turn right
                    }
                }
                obj.updateMovableObject();
                stage.setSurfaceHeight(obj);
            }
            base.Update(gameTime);  // MovableMesh's Update();
        }

        private Object3D computeSeparation(Object3D obj)
        {
            if (leader == null)
            {
                obj.turnToFace(getCenter(obj));

            }
            else
            {
                obj.turnToFace(leader.Translation);

            }

            obj.Yaw += (float)Math.PI;

            return obj;
        }
        private Object3D computeAlignment(Object3D obj)
        {
            if (leader == null)
            {

                obj.Yaw = getYaw(obj);
            }
            else
            {
                obj.Yaw = leader.Yaw;
            }

            return obj;
        }
        private Object3D computeCohesion(Object3D obj)
        {
            if (leader == null)
            {
                obj.turnToFace(getCenter(obj));
            }
            else
            {
                obj.turnToFace(leader.Translation);
            }
            return obj;
        }

        public Vector3 getCenter(Object3D o)
        {
            Vector3 v = new Vector3();
            int neighborCount = 0;
            foreach (Object3D obj in instance)
            {
                float dist = Vector3.Distance(obj.Translation, o.Translation);
                if (obj != o)
                {

                    if (dist < 2000)
                    {
                        v.X += o.Translation.X;
                        v.Z += o.Translation.Z;
                        neighborCount++;
                    }
                }
            }
            if (neighborCount == 0)
                return v;
            v.X /= neighborCount;
            v.Z /= neighborCount;
            v.Normalize();
            return v;
        }

        public float getYaw(Object3D o)
        {
            float yaw = 0.0f;
            int neighborCount = 0;
            foreach (Object3D obj in instance)
            {
                if (obj != o)
                {
                    if (Vector3.Distance(obj.Translation, o.Translation) < 2000)
                    {
                        yaw += o.Yaw;
                        neighborCount++;
                    }
                }
            }
            if (neighborCount == 0)
                return yaw;
            yaw /= neighborCount;
            return yaw;
        }
        public double toggleFlocking() // flocking percent
        {
            while (flockingPercent != 99)
            {
                flockingPercent += 33;
                return flockingPercent;

            }

            flockingPercent = 0;
            return flockingPercent;
        }


        public Object3D Leader
        {
            get { return leader; }
            set { leader = value; }
        }

    }
}
