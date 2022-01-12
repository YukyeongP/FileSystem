using System.Collections.Generic;

namespace FileSystem
{
    class Filesystem
    {
        public Node RootNode;
        public Dictionary<string, Node> NodeTree;
        public Filesystem()
        {
            RootNode = new Node();
            NodeTree = new Dictionary<string, Node>();
        }
    
        public Node GetNode(string path)
        {
            var node = new Node();

            return NodeTree[path.ToUpper()];
        }
    }
}