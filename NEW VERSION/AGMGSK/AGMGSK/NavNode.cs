﻿/* Projec 2 Comp 565
 * Arnold Santos   <arnold2020@yahoo.com>
 * Cesar Zalzalah  <7701707@gmail.com>
 * Dani Odicho     <dannykaka2009@hotmail.com>
 * Ernie Ledezma   <eledezma518@gmail.com>
/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file NavNode.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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
/// A WayPoint or Marker to be used in path following or path finding.
/// Four types of WAYPOINT:
/// <list type="number"> WAYPOINT, a navigatable terrain vertex </list>
/// <list type="number"> PATH, a node in a path (could be the result of A*) </list>
/// <list type="number"> OPEN, a possible node to follow in an A*path</list>
/// <list type="number"> CLOSED, a node that has been evaluated by A* </list>
 
/// 
/// 2/14/2012  last update
/// </summary>
public class NavNode : IComparable<NavNode> {
   public enum NavNodeEnum {VERTEX, WAYPOINT, PATH, OPEN, CLOSED };
   private double distance;  // can be used with A* path finding.
   private Vector3 translation;
   private NavNodeEnum navigatable;
   private Vector3 nodeColor;

   //Added Stuff
   public double cost; // function of distance
   public double distanceToSource, distanceToGoal;
   public List<NavNode> adjacent;
   public NavNode pathPredecessor;

// constructors

   /// <summary>
   /// Make a VERTEX NavNode
   /// </summary>
   /// <param name="pos"> location of WAYPOINT</param>
   public NavNode(Vector3 pos) {
      translation = pos;
      Navigatable = NavNodeEnum.WAYPOINT;
      adjacent = new List<NavNode>();
   }

   

   /// <summary>
   /// Make a WAYPOINT and set its Navigational type
   /// </summary>
   /// <param name="pos"> location of WAYPOINT</param>
   /// <param name="nType"> Navigational type {VERTEX, WAYPOINT, A_STAR, PATH} </param>
   public NavNode(Vector3 pos, NavNodeEnum nType) {
      translation = pos;
      Navigatable = nType;
      adjacent = new List<NavNode>();

   }

   public NavNode(Vector3 pos, NavNodeEnum nType, int dist)
   {
       translation = pos;
       Navigatable = nType;
       adjacent = new List<NavNode>();
       distance = dist;

   }
// properties

   public Vector3 NodeColor {
      get { return nodeColor; }}

   public Double Distance {
      get { return distance; }
      set { distance = value; }
      }

   /// <summary>
   /// When changing the Navigatable type the WAYPOINT's nodeColor is 
   /// also updated.
   /// </summary>
   public NavNodeEnum Navigatable {
      get { return navigatable; }
      set { navigatable = value; 
            switch (navigatable) {
               case NavNodeEnum.WAYPOINT : nodeColor = Color.Yellow.ToVector3(); break;  // yellow
               case NavNodeEnum.PATH     : nodeColor = Color.Blue.ToVector3();   break;  // blue
               case NavNodeEnum.OPEN     : nodeColor = Color.White.ToVector3();  break;  // white
               case NavNodeEnum.CLOSED   : nodeColor = Color.Red.ToVector3();    break;  // red
               case NavNodeEnum.VERTEX:nodeColor = Color.Green.ToVector3();break;  // red


               }
            }} 

   public Vector3 Translation {
      get { return translation; }
      }

// methods

   /// <summary>
   /// Useful in A* path finding 
   /// when inserting into an min priority queue open set ordered on distance
   /// </summary>
   /// <param name="n"> goal node </param>
   /// <returns> usual comparison values:  -1, 0, 1 </returns>
   public int CompareTo(NavNode n) {
      if (distance < n.Distance)       return -1;
      else if (distance > n.Distance)  return  1;
      else                             return  0;
      }
      
   }
}
