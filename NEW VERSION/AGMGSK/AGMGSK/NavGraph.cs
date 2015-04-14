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
    //Class that creates the quadtree nodes and handles path finding with A*
    class NavGraph : DrawableGameComponent
    {
        Dictionary<String, NavNode> graph; // Key "x:z"
        List<NavNode> open, closed, path;
        List<NavNode> aStarPath;
        List<NavNode> allNodes;
        public Stage stage;         // instance of the stage
        int nodesSpacing;
        
        // Constructor that gets the instance of the stage and creates the quad tree nodes
        public NavGraph(Stage s) : base(s)
        {
            stage = s;
            allNodes = new List<NavNode>();                 // instantiate the nodes in the quad tree
            createQuadTreeNodes(2,2,510,510);
            nodesSpacing = 2;
        }

        // Creates the quad tree nodes
        public void createQuadTreeNodes(int x1, int y1, int x2, int y2)
        {

            if (Math.Abs(x1 - x2) > nodesSpacing)
            {
                // if there is an object in the area, break to four quadrants and recurse
                if (objectExists(x1, y1, x2, y2))
                {
                    createQuadTreeNodes(x1, y1, (x2 + x1) / 2, (y2 + y1) / 2);     //top left
                    createQuadTreeNodes((x2 + x1) / 2, (y2 + y1) / 2, x2, y2);     // bottom right
                    createQuadTreeNodes(x1, (y2 + y1) / 2, (x2 + x1) / 2, y2);     // bottom left
                    createQuadTreeNodes((x2 + x1) / 2, y1, x2, (y2 + y1) / 2);     // top right
                }
                else
                {
                    // Creates 8 nodes on the edges of the rectangle , and one node in the center
                    allNodes.Add(new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1, y1), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2, y2), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1, y2), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2, y1), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, y1 * stage.Spacing), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, y2 * stage.Spacing), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1 * stage.Spacing, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    allNodes.Add(new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2 * stage.Spacing, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                }

            }
            

        }
        // Checks if there is a collidable object in the quadrant
        public bool objectExists(int x1, int y1, int x2, int y2)
        {
            x1 *= stage.Spacing;
            x2 *= stage.Spacing;
            y1 *= stage.Spacing;
            y2 *= stage.Spacing;

            // Check all collidable object who are either wall or temple
            foreach (Object3D obj in stage.Collidable) {
                if(obj.Name.Contains("wall")||obj.Name.Contains("temple")){
                    //if the object or its buounding sphere is within the boundaries of the rectangle return true
                    if((obj.Translation.X+obj.ObjectBoundingSphereRadius>x1&&obj.Translation.X-obj.ObjectBoundingSphereRadius<x2)&&(obj.Translation.Z+obj.ObjectBoundingSphereRadius>y1&&obj.Translation.Z-obj.ObjectBoundingSphereRadius<y2)){
                        return true;
                    }
                }
            }
            // if no objects were found return base
            return false;
        }

        //Draws the nav nodes
        public override void Draw(GameTime gameTime)
        {
            Matrix[] modelTransforms = new Matrix[stage.WayPoint3D.Bones.Count];
            foreach (NavNode navNode in allNodes)
            {
                // draw the Path markers
                foreach (ModelMesh mesh in stage.WayPoint3D.Meshes)
                {
                    stage.WayPoint3D.CopyAbsoluteBoneTransformsTo(modelTransforms);
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        if (stage.Fog)
                        {
                            effect.FogColor = Color.CornflowerBlue.ToVector3();
                            effect.FogStart = stage.FogStart;
                            effect.FogEnd = stage.FogEnd;
                            effect.FogEnabled = true;
                        }
                        else
                            effect.FogEnabled = false;
                        effect.DirectionalLight0.DiffuseColor = navNode.NodeColor;
                        effect.AmbientLightColor = navNode.NodeColor;
                        effect.DirectionalLight0.Direction = stage.LightDirection;
                        effect.DirectionalLight0.Enabled = true;
                        effect.View = stage.View;
                        effect.Projection = stage.Projection;
                        effect.World = Matrix.CreateTranslation(navNode.Translation) * modelTransforms[mesh.ParentBone.Index];
                    }
                    stage.setBlendingState(true);
                    mesh.Draw();
                    stage.setBlendingState(false);
                }
            }
        }
    }
}
