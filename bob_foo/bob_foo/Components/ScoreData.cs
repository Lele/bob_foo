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
        [Serializable]
        public struct HighScoreData
        {
            public string[] PlayerName;
            public int[] Score;
            

            public int Count;

            public HighScoreData(int count)
            {
                PlayerName = new string[count];
                Score = new int[count];
                Count = count;
            }
        }

        public static void SaveHighScores(HighScoreData data, string filename)
        {
            // Get the path of the save game
            string fullpath = filename;

            // Open the file, creating it if necessary
            Stream stream = TitleContainer.OpenStream(fullpath);
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
            Stream stream = TitleContainer.OpenStream(fullpath);
            
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

        public static void SaveHighScore(string name, int score)
        {
            // Create the data to save
            HighScoreData data = LoadHighScores("highscore.xml");

            int scoreIndex = -1;
            for (int i = 0; i < data.Count; i++)
            {
                if (score > data.Score[i])
                {
                    scoreIndex = i;
                    break;
                }
            }

            if (scoreIndex > -1)
            {
                //New high score found ... do swaps
                for (int i = data.Count - 1; i > scoreIndex; i--)
                {
                    data.PlayerName[i] = data.PlayerName[i - 1];
                    data.Score[i] = data.Score[i - 1];
                   
                }

                data.PlayerName[scoreIndex] = name; //Retrieve User Name Here
                data.Score[scoreIndex] = score;
                

                SaveHighScores(data, "highscore.xml");
            }
        }


    }
}
