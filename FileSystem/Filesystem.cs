using System.Collections.Generic;

namespace MD.FS
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
            return NodeTree[path.ToUpper()];
        }

        // syntatic sugar
        public Node this[string path]
        {
            get => NodeTree[path.ToUpper()];
        }
    }
}