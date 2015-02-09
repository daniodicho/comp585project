/*  
    Copyright (C) 2015 G. Michael Barnes
 
    The file Wall.cs is part of AGMGSKv6 a port and update of AGXNASKv5 from
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
/// A collection of brick.x Models. 
/// Used for path finding and obstacle avoidance algorithms
/// 
/// 2/1/2015 last changed
/// </summary>
public class Wall : Model3D {

public Wall(Stage theStage, string label, string meshFile)  : base(theStage, label, meshFile) {
   isCollidable = true;
   int spacing = stage.Terrain.Spacing;
   Terrain terrain = stage.Terrain;
	// brick[x,z] vertex positions on terrain
	int [,] brick = { // "just another brick in the wall", Pink Floyd 
		{450, 450}, {451, 450}, {452, 450}, {453, 450}, {454, 450}, {455, 450}, {456, 450},  // 7 right
		{457, 443}, {457, 444}, {457, 445}, {457, 446}, {457, 447}, {457, 448}, {457, 449}, {457, 450}, {457, 451}, 
			{457, 452}, {457, 453}, {457, 454}, {457, 455}, {457, 456}, {457, 457}, {457, 458}, {457, 459}, {457, 460}, // 18 down
		{451, 460}, {451, 459}, {451, 458}, {451, 457},  // 4 up
		{451, 456}, {450, 456}, {449, 456}, {448, 456}, {447, 456}, {446, 456}, {445, 456}, {444, 456}, // 8 left
		{444, 455}, {444, 454}, {444, 453}, {444, 452}, {444, 451}, {444, 450}, {444, 449}, {444, 448}, {444, 447}, 
			{444, 446}, {444, 445}, {444, 444},  // 12 up
		{444, 444}, {445, 444}, {446, 444}, {447, 444}, {448, 444}, {449, 444}, {450, 444}, {451, 444},  // 8 right
		{451, 444}, {451, 443} // 2 up
		};
	for (int i = 0; i < brick.GetLength(0); i++) {
		int xPos = brick[i, 0]; 
		int zPos = brick[i, 1];
		addObject(new Vector3(xPos * spacing, terrain.surfaceHeight(xPos, zPos), zPos * spacing), Vector3.Up, 0.0f); }
		}
	}
}