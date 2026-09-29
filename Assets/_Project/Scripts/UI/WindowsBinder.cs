using _Project.Scripts.NetworkLayer.Client;
using _Project.Scripts.UI.Cell;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.UI
{
    public class WindowsBinder : MonoBehaviour
    {
        [Inject]
        public void Construct(
            [Key(0)] PlayerWindow window0,
            [Key(1)] PlayerWindow window1,
            [Key(0)] ClientSession session0,
            [Key(1)] ClientSession session1,
            CellInstantiator cells)
        {
            window0.Bind(session0, cells);
            window1.Bind(session1, cells);
        }
    }
}
