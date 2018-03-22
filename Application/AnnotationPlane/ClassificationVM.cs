using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreSampleAnnotation.AnnotationPlane
{
    public class SelectionTreeNode
    {
        /// <summary>
        /// the array of the path for getting to the top of the tree. [0] elem is a tree root. The last elem is direct parent
        /// </summary>    
        public NonLeafTreeNodeVM[] ChainToTop { get; set; }

        /// <summary>
        /// the array of the path for getting to the top of the tree. [0] elem is a tree root. The last elem is direct parent
        /// </summary>    
        public SelectionTreeNode(NonLeafTreeNodeVM[] chainToTop)
        {
            ChainToTop = chainToTop;
        }
    }

    /// <summary>
    /// Identifes an intermediate node in class choice tree
    /// </summary>
    public class NonLeafTreeNodeVM : SelectionTreeNode
    {
        /// <summary>
        /// How the group is presented in the UI
        /// </summary>
        public string GroupName { get; private set; }
        /// <summary>
        /// The direct descendent
        /// </summary>
        public SelectionTreeNode[] Children { get; set; }

        /// <param name="groupName">How the group is presented in the UI</param>
        /// <param name="chainToTop">the array of the path for getting to the top of the tree. [0] elem is a tree root. The last elem is direct parent</param>
        public NonLeafTreeNodeVM(string groupName, NonLeafTreeNodeVM[] chainToTop) : base(chainToTop)
        {
            GroupName = groupName;
            Children = new SelectionTreeNode[0];
        }
    }

    /// <summary>
    /// Identifies a leaf node in class choice tree
    /// </summary>
    public class LeafTreeNode : SelectionTreeNode
    {
        public LayerClassVM AssociatedClass { get; private set; }

        /// <param name="chainToTop">the array of the path for getting to the top of the tree. [0] elem is a tree root. The last elem is direct parent</param>
        public LeafTreeNode(LayerClassVM associatedClass, NonLeafTreeNodeVM[] chainToTop) : base(chainToTop)
        {
            AssociatedClass = associatedClass;
        }
    }

    /// <summary>
    /// Builds a choice tree
    /// </summary>
    public interface ITreeBuilder {
        NonLeafTreeNodeVM BuildChoiceTree(string rootGroupName, LayerClassVM[] classes);
    }


    public class ClassificationVM : ViewModel
    {
        private ClassificationLayerVM layerVM;
        public ClassificationLayerVM LayerVM
        {
            get { return layerVM; }
            set
            {
                if (layerVM != value)
                {
                    layerVM = value;

                    var classes = layerVM.PossibleClasses;

                    if (choiceCache.ContainsKey(classes))
                        currentlyObservedNode = choiceCache[classes];
                    else
                    {                        
                        var choiceTree = ChoiceTreeBuilder.BuildChoiceTree(layerVM.PropertyName, classes);
                        choiceCache.Add(classes,choiceTree);
                        currentlyObservedNode = choiceTree;
                    }
                    RaisePropertyChanged(nameof(LayerVM));
                    RaisePropertyChanged(nameof(CurrentlyObservedNode));
                }
            }
        }
        
        private NonLeafTreeNodeVM currentlyObservedNode;
        /// <summary>
        /// Which group in class hierarchy is now on the screen
        /// </summary>
        public NonLeafTreeNodeVM CurrentlyObservedNode
        {
            get { return currentlyObservedNode; }
            set
            {
                if (currentlyObservedNode != value)
                {
                    currentlyObservedNode = value;
                    choiceCache[layerVM.PossibleClasses] = currentlyObservedNode;
                    RaisePropertyChanged(nameof(CurrentlyObservedNode));                    
                }
            }
        }

        public ITreeBuilder ChoiceTreeBuilder { get; private set; }

        public ClassificationVM(ITreeBuilder choiceTreeBuilder) {
            ChoiceTreeBuilder = choiceTreeBuilder;
        }

        /// <summary>
        /// Caches the last view of the tree.
        /// </summary>
        private Dictionary<LayerClassVM[], NonLeafTreeNodeVM> choiceCache = new Dictionary<LayerClassVM[], NonLeafTreeNodeVM>();

        private ICommand closeCommand;
        public ICommand CloseCommand
        {
            get { return closeCommand; }
            set
            {
                if (closeCommand != value)
                {
                    closeCommand = value;
                    RaisePropertyChanged(nameof(CloseCommand));
                }
            }
        }

        private ICommand classSelectedCommand;
        public ICommand ClassSelectedCommand
        {
            get { return classSelectedCommand; }
            set
            {
                if (classSelectedCommand != value)
                {
                    classSelectedCommand = value;
                    RaisePropertyChanged(nameof(ClassSelectedCommand));
                }
            }
        }

        private ICommand groupSelectedCommand;
        public ICommand GroupSelectedCommand {
            get { return groupSelectedCommand; }
            set {
                if (groupSelectedCommand != value) {
                    groupSelectedCommand = value;
                }
            }
        }

        private bool isVisisble;
        public bool IsVisible
        {
            get { return isVisisble; }
            set
            {
                if (isVisisble != value)
                {
                    isVisisble = value;
                    RaisePropertyChanged(nameof(IsVisible));
                }
            }
        }      
    }    

    /// <summary>
    /// Condiders class ID to hold the path in choice tree. Tree "forks" are separated by special char.
    /// </summary>
    public class IDEncodedTreeBuilder : ITreeBuilder
    {
        /// <summary>
        /// Char to use as splitter of levels during ID parsing
        /// </summary>
        public char Splitter { get; private set; }

        /// <param name="splitter">Char to use as splitter of levels during ID parsing</param>
        public IDEncodedTreeBuilder(char splitter) {
            Splitter = splitter;
        }

        public NonLeafTreeNodeVM BuildChoiceTree(string rootGroupName, LayerClassVM[] classes)
        {
            NonLeafTreeNodeVM root = new NonLeafTreeNodeVM(rootGroupName, new NonLeafTreeNodeVM[0]);
            foreach (LayerClassVM vm in classes)
            {
                string[] path = vm.ID.Split(new char[] { Splitter });
                PushClassToTree(root, path, vm);
            }
            return root;
        }

        /// <summary>
        /// Adds a class <paramref name="data"/> to the <paramref name="tree"/> (modifies the content of the tree) to the location in tree defined by <paramref name="classLocationPath"/>
        /// </summary>
        /// <param name="tree">What tree to push to</param>
        /// <param name="classLocationPath">A sequence of group names. The last element is an ID of the leaf node</param>
        /// <param name="data">What data to save in a new leaf node</param>
        private static void PushClassToTree(NonLeafTreeNodeVM tree, string[] classLocationPath, LayerClassVM data)
        {
            if (classLocationPath.Length == 0)
                throw new ArgumentException("classLocationPath must contain at least one element");

            var children = new List<SelectionTreeNode>(tree.Children);

            if (classLocationPath.Length == 1)
            {
                //time to create leaf

                //constructing path to top
                List<NonLeafTreeNodeVM> pathToTop = new List<NonLeafTreeNodeVM>(tree.ChainToTop);
                pathToTop.Add(tree);

                LeafTreeNode leaf = new LeafTreeNode(data, pathToTop.ToArray());
                children.Add(leaf);
                tree.Children = children.ToArray();
            }
            else
            {
                //not a leaf node
                string groupName = classLocationPath[0];
                string[] tail = classLocationPath.Skip(1).ToArray();
                //is the non-leaf with the required group name already exists?
                var nonLeafs = tree.Children.Where(g => g is NonLeafTreeNodeVM).Select(g => (NonLeafTreeNodeVM)g);
                NonLeafTreeNodeVM node = nonLeafs.FirstOrDefault(g => g.GroupName == groupName);
                if (node == null)
                {
                    //constructing path to top
                    List<NonLeafTreeNodeVM> pathToTop = new List<NonLeafTreeNodeVM>(tree.ChainToTop);
                    pathToTop.Add(tree);


                    //we need to create a new non-leaf node as node with required group name is not exists
                    node = new NonLeafTreeNodeVM(groupName, pathToTop.ToArray());
                    children.Add(node);
                    tree.Children = children.ToArray();
                }
                PushClassToTree(node, tail, data);
            }
        }
    }
}
