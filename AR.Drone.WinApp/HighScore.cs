using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AR.Drone.WinApp
{
    public class HighScore
    {
        #region Variables

        List<HighScoreRecord> _highScores;

        #endregion

        #region Properties

        public List<HighScoreRecord> HighScores
        {
            get
            {
                return _highScores;
            }

            set
            {
                _highScores = value;
            }
        }

        #endregion

        #region Public Func

        public void SaveHighScore()
        {
            var serializer = new XmlSerializer(_highScores.GetType(), "HighScores.Scores");
            using (var writer = new StreamWriter("highscores.xml", false))
            {
                serializer.Serialize(writer.BaseStream, _highScores);
            }
        }

        public void LoadHighScore()
        {
            _highScores = new List<HighScoreRecord>();
            var serializer = new XmlSerializer(_highScores.GetType(), "HighScores.Scores");
            object obj;
            try
            {
                using (var reader = new StreamReader("highscores.xml"))
                {
                    obj = serializer.Deserialize(reader.BaseStream);
                }
                _highScores = (List<HighScoreRecord>)obj;
            }
            catch (Exception e)
	        {
                _highScores.Add(new HighScoreRecord() { Name = "test", Score = 1000 });
            }

            _highScores = _highScores.OrderBy(x => x.Score).ToList();
        }

        public void AddToHighScore(string name, int score)
        {
            _highScores.Add(new HighScoreRecord() { Name = name, Score = score });
            _highScores = _highScores.OrderBy(x => x.Score).ToList();
        }

        #endregion
    }

    [Serializable]
    public class HighScoreRecord
    {
        public int Score { get; set; }
        public string Name { get; set; }
    }
}
