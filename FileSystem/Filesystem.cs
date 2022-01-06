using System.Collections.Generic;

namespace FileSystem
{
    class Filesystem
    {
        //public Dictionary<string, Node> NodeTree { get; private set; }
        public Node RootNode;
        public Filesystem()
        {

        }
        
        //
        public Node GetNode(string path)
        {
            
            var node = new Node(true);

            return node;
        }
    }
}