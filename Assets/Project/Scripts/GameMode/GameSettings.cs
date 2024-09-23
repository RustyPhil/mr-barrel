using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Serializable]
    private class CheckpointSettings
    {
        [SerializeField] private string name;
        public string rewardType;
        public bool victory = false;
        public int score = 500;

        private int id = 0;
        public int ID => id;

        public void Init() => id = Animator.StringToHash(name);
    }

    [SerializeField] private CheckpointSettings[] checkpoints;
    private Dictionary<int, CheckpointSettings> checkpointHash = new Dictionary<int, CheckpointSettings>();

    private void Start()
    {
        foreach (CheckpointSettings entry in checkpoints)
        {
            entry.Init();
            checkpointHash[entry.ID] = entry;
        }

        GameState.OnPointReached += PointReached;
    }
    
    private void OnDestroy() => GameState.OnPointReached -= PointReached;

    private void PointReached(int point)
    {
        if(!checkpointHash.ContainsKey(point))
            return;

        CheckpointSettings entry = checkpointHash[point];
        if(entry.score != 0)
            GameState.AddScore(entry.score, entry.rewardType);
        
        if(entry.victory)
            GameState.EndGame(true);
    }
}
