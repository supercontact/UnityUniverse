using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MineSweeperUI : MonoBehaviour {

    public Text mineCounter;
    public Text timer;

    public void SetMineCount(int remaining, int total) {
        mineCounter.text = remaining + "/" + total;
        mineCounter.color = remaining < 0 ? Color.red : Color.white; 
    }

    public void SetTime(int timeInSeconds) {
        timer.text = timeInSeconds.ToString();
    }
}
