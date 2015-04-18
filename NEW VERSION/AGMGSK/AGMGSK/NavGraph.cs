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
    public class NavGraph : DrawableGameComponent
    {
        int totalAdjacents = 0;
        public Dictionary<String, NavNode> graph; // Key "x:z"
        List<NavNode> open, closed, path;
        List<NavNode> aStarPath;
        public bool pathComplete = false;
        List<NavNode> allNodes;
        public Stage stage;         // instance of the stage
        int nodesSpacing;
        
        // Constructor that gets the instance of the stage and creates the quad tree nodes
        public NavGraph(Stage s)
            : base(s)
        {
            nodesSpacing = 125;
            stage = s;
            graph = new Dictionary<String, NavNode>();                 // instantiate the nodes in the quad tree
            allNodes = new List<NavNode>();
            createQuadTreeNodes(2 * stage.Spacing, 2 * stage.Spacing, 510 * stage.Spacing, 510 * stage.Spacing);
            //          setAllAdjacents();
            //          cleanUp();
            

        }

        // Creates the quad tree nodes
        public void createQuadTreeNodes(int x1, int y1, int x2, int y2)
        {
            if (Math.Abs(x1 - x2) > nodesSpacing)
            {
                // if there is an object in the area, break to four quadrants and recurse
                if (objectExists(x1, y1, x2, y2)||(x2-x1>50*stage.Spacing))
                {
                    createQuadTreeNodes(x1, y1, (x2 + x1) / 2, (y2 + y1) / 2);     //top left
                    createQuadTreeNodes((x2 + x1) / 2, (y2 + y1) / 2, x2, y2);     // bottom right
                    createQuadTreeNodes(x1, (y2 + y1) / 2, (x2 + x1) / 2, y2);     // bottom left
                    createQuadTreeNodes((x2 + x1) / 2, y1, x2, (y2 + y1) / 2);     // top right
                }
                else
                {
                    List<NavNode> currentNodes = new List<NavNode>();
                    // Creates 8 nodes on the edges of the rectangle , and one node in the center
                    if (!graph.ContainsKey(skey(x1  , y1  )))
                    {
                        NavNode node1 = new NavNode(new Vector3(x1  , stage.surfaceHeight(x1  , y1  ), y1  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x1  , y1  ), node1);
                        currentNodes.Add(node1);
                    }
                    if (!graph.ContainsKey(skey(x2  , y2  )))
                    {
                        NavNode node2 = new NavNode(new Vector3(x2  , stage.surfaceHeight(x2  , y2  ), y2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x2  , y2  ), node2);
                        currentNodes.Add(node2);
                    }
                    if (!graph.ContainsKey(skey(x1  , y2  )))
                    {
                        NavNode node3 = new NavNode(new Vector3(x1  , stage.surfaceHeight(x1  , y2  ), y2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x1  , y2  ), node3);
                        currentNodes.Add(node3);
                    }
                    if (!graph.ContainsKey(skey(x2  , y1  )))
                    {
                        NavNode node4 = new NavNode(new Vector3(x2  , stage.surfaceHeight(x2  , y1  ), y1  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x2  , y1  ), node4);
                        currentNodes.Add(node4);
                    }
                    if (!graph.ContainsKey(skey((x1 + x2) / 2  , (y1 + y2) / 2  )))
                    {
                        NavNode node5 = new NavNode(new Vector3(((x1 + x2) / 2)  , stage.surfaceHeight((x1 + x2)   / 2, (y1 + y2)   / 2), (y1 + y2) / 2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey((x1 + x2) / 2  , (y1 + y2) / 2  ), node5);
                        currentNodes.Add(node5);
                    }
                    if (!graph.ContainsKey(skey((x1 + x2) / 2  , y1  )))
                    {
                        NavNode node6 = new NavNode(new Vector3(((x1 + x2) / 2)  , stage.surfaceHeight((x1 + x2)   / 2, y1  ), y1  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey((x1 + x2) / 2  , y1  ), node6);
                        currentNodes.Add(node6);
                    }
                    if (!graph.ContainsKey(skey((x1 + x2) / 2  , y2  )))
                    {
                        NavNode node7 =new NavNode(new Vector3(((x1 + x2) / 2)  , stage.surfaceHeight((x1 + x2)   / 2, y2  ), y2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey((x1 + x2) / 2  , y2  ), node7);
                        currentNodes.Add(node7);
                    }
                    if (!graph.ContainsKey(skey(x1  , (y1 + y2) / 2  )))
                    {
                        NavNode node8 = new NavNode(new Vector3(x1  , stage.surfaceHeight(x1  , (y1 + y2)   / 2), (y1 + y2) / 2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x1  , (y1 + y2) / 2  ), node8);
                        currentNodes.Add(node8);
                    }
                    if (!graph.ContainsKey(skey(x2  , (y1 + y2) / 2  )))
                    {
                        NavNode node9 = new NavNode(new Vector3(x2  , stage.surfaceHeight(x2  , (y1 + y2)   / 2), (y1 + y2) / 2  ), NavNode.NavNodeEnum.PATH,Math.Abs(x1-x2)/2);
                        graph.Add(skey(x2  , (y1 + y2) / 2  ), node9);
                        currentNodes.Add(node9);
                    }
                    foreach (NavNode cur in currentNodes)
                    {
                        foreach (KeyValuePair<String, NavNode> nav in graph)
                        {
                            if (Vector3.Distance(cur.Translation,nav.Value.Translation) < 1.5  * Math.Abs(x1 - x2) / 2)
                            {
                                if (cur.Translation != nav.Value.Translation)
                                {
                                    if(cur.adjacent.Count>100)
                                    {

                                    }
                                    connect(cur, nav.Value);
                                }

                            }
                        }
                    }

                    

            /*        foreach (KeyValuePair<String, NavNode> node in graph)
                    {
                        foreach(NavNode cur in currentNodes)
                        {
                            if(Math.Sqrt(Math.Pow(node.Value.Translation.X+cur.Translation.X,2)+Math.Pow(node.Value.Translation.Z+cur.Translation.Z,2))<1.5*stage.Spacing*(x1+x2)/2){
                                if (cur != node.Value)
                                {
                                    connect(cur, node.Value);
                                }
                            }
                        }
                    }*/
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
           

            // Check all collidable object who are either wall or temple
            foreach (Object3D obj in stage.Collidable) {
                if(obj.Name.Contains("wall")||obj.Name.Contains("temple")){
                    //if the object or its buounding sphere is within the boundaries of the rectangle return true
                    if((obj.Translation.X+obj.ObjectBoundingSphereRadius*2>x1&&obj.Translation.X-obj.ObjectBoundingSphereRadius*2<x2)&&(obj.Translation.Z+obj.ObjectBoundingSphereRadius*2>y1&&obj.Translation.Z-obj.ObjectBoundingSphereRadius*2<y2)){
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

        public Path aStar(Vector3 startPosition, NavNode destination)
        {
            NavNode source = null;
            float closest = Int64.MaxValue;
            foreach (KeyValuePair<String, NavNode> node in graph)
            {
                float d = Vector3.Distance(startPosition,new Vector3(node.Value.Translation.X,node.Value.Translation.Y,node.Value.Translation.Z));
                if(d<closest)
                {
                    source = node.Value;
                    closest = d;
                }
            }

            pathComplete = false;
            open = new List<NavNode>();
            closed = new List<NavNode>();
            List<NavNode> p = new List<NavNode>();
            Path path= null;
            NavNode cur= source;
            cur.cost = 0;
            
            open.Add(cur);
            open.Sort(delegate(NavNode n1, NavNode n2)
                {
                    return n1.cost.CompareTo(n2.cost);
                });
            while (!(open.Count == 0))
            {

                cur = open[0];
                open.RemoveAt(0);
                if (Vector3.Distance(cur.Translation,destination.Translation)<=cur.Distance)
                {
                    break;
                }
                    closed.Add(cur);
     //               cur.Navigatable = NavNode.NavNodeEnum.CLOSED;

                foreach (NavNode node in cur.adjacent)
                {
                    if (!open.Contains(node) && !closed.Contains(node))
                    {
                        // keep track of the path
                        node.pathPredecessor = cur;

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
                        node.Navigatable = NavNode.NavNodeEnum.OPEN;
                        
                    }
                }
                open.Sort(delegate(NavNode n1, NavNode n2)
                {
                    return n1.cost.CompareTo(n2.cost);
                });


            }
            //    int count=0;
            p.Add(new NavNode(destination.Translation));
                while (Vector3.Distance(cur.Translation, source.Translation) != 0.0)
                {
              //      count++;
                    p.Add(cur);
                    cur.Navigatable = NavNode.NavNodeEnum.PATH;
                    cur=cur.pathPredecessor;
                }
            /*    int[,] pathValues=new int[count,2];
                while (count != 1)
                {
                    count--;
                    pathValues[count,0] = (int)p[count].Translation.X;
                    pathValues[count,1] = (int)p[count].Translation.Z;
                }*/

                path = new Path(stage, p, Path.PathType.REVERSE);
                path = path.reversePath(path);
                pathComplete = true;
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
                node1.adjacent.Add(node2);
                totalAdjacents++;
            }
            if (!node2.adjacent.Contains(node1))
            {
                totalAdjacents++;
                node2.adjacent.Add(node1);
            }
        }

        
        //Draws the nav nodes
        public override void Draw(GameTime gameTime)
        {
            Matrix[] modelTransforms = new Matrix[stage.WayPoint3D.Bones.Count];
            foreach (KeyValuePair<String, NavNode> navNode in graph)
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
