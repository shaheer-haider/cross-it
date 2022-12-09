using UnityEngine;
using System.Collections;
using System;

namespace SgLib
{

    public class DailyRewardController : MonoBehaviour
    {
        public static DailyRewardController Instance { get; private set; }

        public TimeSpan TimeUntilReward { get; private set; }

        [Header("Check to disable Daily Reward Feature")]
        public bool disable;

        [Header("Daily Reward Config")]
        [Tooltip("Number of hours between 2 rewards")]
        public int rewardIntervalHours = 6;
        [Tooltip("Number of minues between 2 rewards")]
        public int rewardIntervalMinutes = 0;
        [Tooltip("Number of seconds between 2 rewards")]
        public int rewardIntervalSeconds = 0;
        public float minRewardValue = 20;
        public float maxRewardValue = 50;

        private DateTime nextRewardTime;
        private const string NextRewardTimePPK = "SGLIB_NEXT_DAILY_REWARD_TIME";

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            nextRewardTime = GetNextRewardTime();
        }

        void Update()
        {
            TimeUntilReward = CalculateTimeUntil(nextRewardTime);
        }

        /// <summary>
        /// Set the next reward time to some time in future determined by the number of hours, minutes and seconds.
        /// </summary>
        public void SetNextRewardTime(int hours, int minutes, int seconds)
        {
            nextRewardTime = DateTime.Now.Add(new TimeSpan(hours, minutes, seconds));
            StoreNextRewardTime(nextRewardTime);
        }

        TimeSpan CalculateTimeUntil(DateTime time)
        {
            return time.Subtract(DateTime.Now);
        }

        void StoreNextRewardTime(DateTime time)
        {
            PlayerPrefs.SetString(NextRewardTimePPK, time.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        DateTime GetNextRewardTime()
        {
            string storedTime = PlayerPrefs.GetString(NextRewardTimePPK, string.Empty);

            if (!string.IsNullOrEmpty(storedTime))
                return DateTime.FromBinary(Convert.ToInt64(storedTime));
            else
                return DateTime.Now;
        }
    }
}
