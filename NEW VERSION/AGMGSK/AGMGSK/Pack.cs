/* Projec 2 Comp 565
 * Arnold Santos   <arnold2020@yahoo.com>
 * Cesar Zalzalah  <7701707@gmail.com>
 * Dani Odicho     <dannykaka2009@hotmail.com>
 * Ernie Ledezma   <eledezma518@gmail.com>
/* 
 * 
 * ideas and research for flocking used from:
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
    /// Pack represents a "flock" of MovableObject3D's Object3Ds.
    /// Usually the "player" is the leader and is set in the Stage's LoadContent().
    /// With no leader, determine a "virtual leader" from the flock's members.
    /// Model3D's inherited List<Object3D> instance holds all members of the pack.
    ///
    /// 2/1/2015 last changed
    /// </summary>
    public class Pack : MovableModel3D
    {
        Object3D leader;
        double flockingPercent = 0.0;
        int intensity = 5;

        /// <summary>
        /// Construct a pack with an Object3D leader
        /// </summary>
        /// <param name="theStage"> the scene </param>
        /// <param name="label"> name of pack</param>
        /// <param name="meshFile"> model of a pack instance</param>
        /// <param name="xPos, zPos">  approximate position of the pack </param>
        /// <param name="aLeader"> alpha dog can be used for flock center and alignment </param>
        public Pack(Stage theStage, string label, string meshFile, int nDogs, int xPos, int zPos, Object3D theLeader)
            : base(theStage, label, meshFile)
        {
            isCollidable = true;
            random = new Random();
            leader = theLeader;
            int spacing = stage.Spacing;
            // initial vertex offset of dogs around (xPos, zPos)
            int[,] position = { { 0, 0 }, { 7, -4 }, { -5, -2 }, { -7, 4 }, { 5, 2 }, { 9, 7 }, { 6, 9 }, { 0, 5 }, { -10, 2 }, { -2, 2 } };
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

                if (random.NextDouble() * 100 < flockingPercent) // if flocking is present there must be leader
                {

                    Vector3 flockingV = obj.Translation += getCohesion(obj) + getSeparation(obj);
                    Vector3 forwardV = getAlignment(obj);

                    flockingV.Normalize();
                    forwardV.Normalize();

                    float alienAngle = MathHelper.ToRadians(1);  //angle in radians

                    if (random.NextDouble() < 0.07)  //as alien separate, cohese, and align with leader, have them randomly turn
                    {
                        if (random.NextDouble() < 0.5) obj.Yaw -= alienAngle; // turn left
                        else obj.Yaw += alienAngle; // turn right
                    }

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



        //ALIGNMENT
        public Vector3 getAlignment(Object3D current)
        {
            float distanceV = Vector3.Distance(current.Translation, leader.Translation);

            Vector3 alignment = Vector3.Zero;

            if (distanceV < 6000) // objecs within this distance be aligned with leader
            {
                alignment.X = leader.Forward.X;
                alignment.Z = leader.Forward.Z;
            }
            alignment *= intensity;
            return alignment;
        }

        //COHESION
        public Vector3 getCohesion(Object3D current)
        {
            Vector3 cohesionV = Vector3.Zero;

            cohesionV = leader.Translation - current.Translation;  //Cohesion vector towards leader

            cohesionV.Normalize();

            cohesionV = cohesionV * intensity * (float)random.NextDouble(); // add intensity; create al arger cohesion force

            return cohesionV;

        }

        //SEPARATION
        public Vector3 getSeparation(Object3D current)
        {
            Vector3 separation = Vector3.Zero;
            float distance = Vector3.Distance(leader.Translation, current.Translation);

            foreach (Object3D obj in instance)
            {
                if (current != obj)
                {
                    Vector3 alienDistance = current.Translation - obj.Translation;

                    if (alienDistance.Length() < 1000)  // if distance between aliens less than 1000 radius then add separation
                    {
                        alienDistance.Normalize();
                        separation += separation + alienDistance;
                    }
                }
            }

            if (distance < 1500) // if distance between alien and leader is less than 1500 add separation
            {
                Vector3 leaderDistance = current.Translation - leader.Translation;
                separation += 5 * Vector3.Normalize(leaderDistance) / (leaderDistance.Length() / 600);

            }

            separation = intensity * separation;
            return separation;
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