using System.Collections.Generic;
using UnityEngine;

namespace MineSweeper {
    public class Globals : MonoBehaviour {

        public static Globals instance;

        public ObserveCamera observeCamera;

        private void Awake() {
            instance = this;
        }

        private void OnDestroy() {
            instance = null;
        }
    }
}