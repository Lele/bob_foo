using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;

namespace bob_foo.Components
{
    public class ScoreData
    {
        public struct LevelScoreData
        {
            public string[] PlayerName;
            public int[] Score;


            public int Count;

            public LevelScoreData(int count)
            {
                PlayerName = new string[count];
                Score = new int[count];
                Count = count;
            }
        }
        [Serializable]
        public struct HighScoreData
        {
           public LevelScoreData[] level;
           public HighScoreData(int count)
           {
               level = new LevelScoreData[3];
               for (int i = 0; i < 3; i++)
                   level[i] = new LevelScoreData(count);
           }
        }

        public static void SaveHighScores(HighScoreData data, string filename)
        {
            // Get the path of the save game
            string fullpath = filename;

            // Open the file, creating it if necessary
            FileStream stream = new FileStream(fullpath,FileMode.Create);
            try
            {
                // Convert the object to XML data and put it in the stream
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                serializer.Serialize(stream, data);
            }
            finally
            {
                // Close the file
                stream.Close();
            }
        }

        public static HighScoreData LoadHighScores(string filename)
        {
            HighScoreData data;

            // Get the path of the save game
            string fullpath = filename;

            // Open the file, creating it if necessary
            FileStream stream = new FileStream(fullpath, FileMode.OpenOrCreate);
            
            try
            {

                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(HighScoreData));
                data = (HighScoreData)serializer.Deserialize(stream);
            }
            finally
            {
                // Close the file
                stream.Close();
            }

            return (data);
        }

        public static void SaveHighScore(string name, int score, int level)
        {
            // Create the data to save
            HighScoreData data = LoadHighScores("highscore.xml");

            int scoreIndex = -1;
            for (int i = 0; i < data.level[level].Count; i++)
            {
                if (score > data.level[level].Score[i])
                {
                    scoreIndex = i;
                    break;
                }
            }

            if (scoreIndex > -1)
            {
                //New high score found ... do swaps
                for (int i = data.level[level].Count - 1; i > scoreIndex; i--)
                {
                    data.level[level].PlayerName[i] = data.level[level].PlayerName[i - 1];
                    data.level[level].Score[i] = data.level[level].Score[i - 1];
                   
                }

                data.level[level].PlayerName[scoreIndex] = name; //Retrieve User Name Here
                data.level[level].Score[scoreIndex] = score;
                

                SaveHighScores(data, "highscore.xml");
            }
        }


    }
}
