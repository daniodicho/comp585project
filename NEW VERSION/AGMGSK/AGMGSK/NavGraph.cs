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
        bool pathComplete;
        List<NavNode> allNodes;
        public Stage stage;         // instance of the stage
        int nodesSpacing;
        
        // Constructor that gets the instance of the stage and creates the quad tree nodes
        public NavGraph(Stage s) : base(s)
        {
            nodesSpacing = 2;
            stage = s;
            graph = new Dictionary<String, NavNode>();                 // instantiate the nodes in the quad tree
            allNodes = new List<NavNode>();
            createQuadTreeNodes(4,4,508,508);
            setAllAdjacents();
  //          cleanUp();

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
                    if(!graph.ContainsKey(skey(x1 * stage.Spacing, y1 * stage.Spacing))){
                        NavNode node1 = new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1, y1), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH);
                        graph.Add(skey(x1 * stage.Spacing, y1 * stage.Spacing), node1);
                    }
                    if (!graph.ContainsKey(skey(x2 * stage.Spacing, y2 * stage.Spacing)))
                    graph.Add(skey(x2 * stage.Spacing, y2 * stage.Spacing), new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2, y2), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey(x1 * stage.Spacing, y2 * stage.Spacing)))
                    graph.Add(skey(x1 * stage.Spacing, y2 * stage.Spacing), new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1, y2), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey(x2 * stage.Spacing, y1 * stage.Spacing)))
                    graph.Add(skey(x2 * stage.Spacing, y1 * stage.Spacing), new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2, y1), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey((x1 + x2) / 2 * stage.Spacing, (y1 + y2) / 2 * stage.Spacing)))
                    graph.Add(skey((x1 + x2) / 2 * stage.Spacing, (y1 + y2) / 2 * stage.Spacing), new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey((x1 + x2) / 2 * stage.Spacing, y1 * stage.Spacing)))
                    graph.Add(skey((x1 + x2) / 2 * stage.Spacing,y1 * stage.Spacing),new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, y1 * stage.Spacing), y1 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey((x1 + x2) / 2 * stage.Spacing,y2 * stage.Spacing)))
                    graph.Add(skey((x1 + x2) / 2 * stage.Spacing,y2 * stage.Spacing),new NavNode(new Vector3(((x1 + x2) / 2) * stage.Spacing, stage.surfaceHeight((x1 + x2) * stage.Spacing / 2, y2 * stage.Spacing), y2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey(x1 * stage.Spacing, (y1 + y2) / 2 * stage.Spacing)))
                    graph.Add(skey(x1 * stage.Spacing,(y1 + y2) / 2 * stage.Spacing),new NavNode(new Vector3(x1 * stage.Spacing, stage.surfaceHeight(x1 * stage.Spacing, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                    if (!graph.ContainsKey(skey(x2 * stage.Spacing, (y1 + y2) / 2 * stage.Spacing)))
                        graph.Add(skey(x2 * stage.Spacing, (y1 + y2) / 2 * stage.Spacing), new NavNode(new Vector3(x2 * stage.Spacing, stage.surfaceHeight(x2 * stage.Spacing, (y1 + y2) * stage.Spacing / 2), (y1 + y2) / 2 * stage.Spacing), NavNode.NavNodeEnum.PATH));
                  
                }

            }
            

        }

        public void cleanUp(){
            foreach (KeyValuePair<String, NavNode> nav in graph)
            {
                foreach (KeyValuePair<String, NavNode> nav2 in graph)
                {

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


        public void setAllAdjacents()
        {
            foreach (KeyValuePair<String, NavNode> node1 in graph)
            {
                List<NavNode> onSameRow = new List<NavNode>();
                onSameRow.Add(node1.Value);
                foreach (KeyValuePair<String, NavNode> node2 in graph)
                {
                    if ((node1.Key != node2.Key) && (node1.Key.Substring(0, node1.Key.IndexOf(':')) == node2.Key.Substring(0, node2.Key.IndexOf(':'))))
                    {
                        onSameRow.Add(node2.Value);
                    }
                }
                onSameRow.Sort();
                for (int i = 0; i < onSameRow.Count - 1; i++)
                {
                    connect(onSameRow[i], onSameRow[i + 1]);
                }
            }

            foreach (KeyValuePair<String, NavNode> node1 in graph)
            {
                List<NavNode> onSameCol = new List<NavNode>();
                onSameCol.Add(node1.Value);
                foreach (KeyValuePair<String, NavNode> node2 in graph)
                {
                    if ((node1.Key != node2.Key) && (node1.Key.Substring(node1.Key.IndexOf(':') + 1) == node2.Key.Substring(node2.Key.IndexOf(':') + 1)))
                    {
                        onSameCol.Add(node2.Value);
                    }
                }
                onSameCol.Sort();
                for (int i = 0; i < onSameCol.Count - 1; i++)
                {
                    connect(onSameCol[i], onSameCol[i + 1]);
                }
            }
        }

        public Path aStar(NavNode source, NavNode destination)
        {
            pathComplete = false;
            open = new List<NavNode>();
            closed = new List<NavNode>();
            List<NavNode> p = new List<NavNode>();
            Path path= null;
            
            open.Add(source);
            while (!(open.Count == 0))
            {
                open.Sort(sortByCost);
                NavNode cur = open[0];
                open.Remove(cur);
                if (cur == destination)
                {
                    pathComplete = true;
                }
                else
                {
                    closed.Add(cur);
                }
                foreach (NavNode node in cur.adjacent)
                {
                    if (!open.Contains(node) && !closed.Contains(node))
                    {
                        // distance from the source to adjacent node
                        node.distanceToSource = cur.distanceToSource +
                            Vector3.Distance(cur.Translation, node.Translation);

                        // heuristic distance from adjacent node to goal
                        node.distanceToGoal =
                            Vector3.Distance(cur.Translation, node.Translation) +
                            Vector3.Distance(node.Translation, destination.Translation);

                        // calculate total cost
                        node.cost = node.distanceToSource + node.distanceToGoal;

                        // Add the node the the open set
                        open.Add(node);

                        // keep track of the path
                        node.pathPredecessor = cur;
                    }
                }
                int count=0;
                while(cur.pathPredecessor!=null){
                    count++;
                    p.Add(cur.pathPredecessor);
                    cur=cur.pathPredecessor;
                }
                int[,] pathValues=new int[count,count];
                while (count != 1)
                {
                    count--;
                    pathValues[count,0] = (int)p[count].Translation.X;
                    pathValues[count,0] = (int)p[count].Translation.Z;
                }

                path = new Path(stage, pathValues, Path.PathType.SINGLE);
            }
            return path;

        }

        public int sortByCost(NavNode n1, NavNode n2)
        {
            return n1.cost.CompareTo(n2.cost);
        }
        public void connect(NavNode node1, NavNode node2)
        {
            if (!node1.adjacent.Contains(node2))
            {
                foreach (NavNode node in node1.adjacent)
                {

                }
                node1.adjacent.Add(node2);
            }
            if (!node2.adjacent.Contains(node1))
            node2.adjacent.Add(node1);
        }
        //Draws the nav nodes
        public override void Draw(GameTime gameTime)
        {
            Matrix[] modelTransforms = new Matrix[stage.WayPoint3D.Bones.Count];
            foreach (KeyValuePair<String, NavNode> navNode in graph)
            {
            
       //     foreach (NavNode navNode in allNodes)
            
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
                        effect.DirectionalLight0.DiffuseColor = navNode.Value.NodeColor;
                        effect.AmbientLightColor = navNode.Value.NodeColor;
                        effect.DirectionalLight0.Direction = stage.LightDirection;
                        effect.DirectionalLight0.Enabled = true;
                        effect.View = stage.View;
                        effect.Projection = stage.Projection;
                        effect.World = Matrix.CreateTranslation(navNode.Value.Translation) * modelTransforms[mesh.ParentBone.Index];
                    }
                    stage.setBlendingState(true);
                    mesh.Draw();
                    stage.setBlendingState(false);
                }
            }
        }

        private String skey(int x, int z)
        {
            return String.Format("{0}:{1}", x, z);
        }
    }
}
