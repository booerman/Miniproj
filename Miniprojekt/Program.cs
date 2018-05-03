using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace Miniprojekt
{
    class Program
    {
        static void Main(string[] args)
        {
            var usopen = new Tournament("Wimbledon", 1993, new DateTime(1993, 05, 26), new DateTime(1993, 06, 05));
            usopen.InitializeTournament();
        }
    }

    public class Tournament
    {
        public string TournamentName { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NoPlayers { get; set; }
        public List<Player> MenSingles { get; set; }
        public List<Player> WomenSingles { get; set; }
        public List<PlayerTeam> MenDoubles { get; set; }
        public List<PlayerTeam> WomenDoubles { get; set; }
        public List<PlayerTeam> MixDoubles { get; set; }
        public List<Referee> FemaleRefs { get; set; }
        public List<Referee> MaleRefs { get; set; }
        public List<Match> ThisRound { get; set; }
        public Referee GameMaster { get; set; }
        public int NoRounds { get; set; }
        public Tournament(string name, int year, DateTime sd, DateTime ed)
        {
            TournamentName = name;
            Year = year;
            StartDate = sd;
            EndDate = ed;
            MaleRefs = Readfile.GetRefs(@"tennis_data\MaleRefs.txt", "Male", Encoding.GetEncoding("iso-8859-1"));
            MenSingles = Readfile.GetPlayers(@"tennis_data\MalePlayer.txt", "Male", Encoding.GetEncoding("iso-8859-1"));
            WomenSingles = Readfile.GetPlayers(@"tennis_data\FemalePlayer.txt", "Female", Encoding.GetEncoding("iso-8859-1"));
            MenDoubles = Readfile.MakeSameGenderTeams(MenSingles, 64);
            WomenDoubles = Readfile.MakeSameGenderTeams(WomenSingles, 64);
            MixDoubles = Readfile.MakeMixedGenderTeams(MenSingles, WomenSingles, 32);
            FemaleRefs = Readfile.GetRefs(@"tennis_data\FermaleRefs.txt", "Female", Encoding.GetEncoding("iso-8859-1"));
            NoPlayers = MenSingles.Count + WomenSingles.Count + (MenDoubles.Count * 2) + (WomenDoubles.Count * 2) + (MixDoubles.Count * 2);
            NoRounds = 0;
        }

        private void SetGameMaster(int index, string gender)
        {
            if (gender == "Male" && index <= MaleRefs.Count)
                GameMaster = MaleRefs[index];
            else if (gender == "Female" && index <= FemaleRefs.Count)
                GameMaster = FemaleRefs[index];
            else
                Console.WriteLine("Referee index is either not within the count of referees, or you spelled the genders \"Female\" or \"Male\" incorretly");


        }

        private void SetThisRoundSingles(List<Player> plist)
        {
            var matchlist = MatchOrganizer.SinglesMatchList(plist);
            if (matchlist[0].Player1.PlayerGender == Player.Gender.Male)
                MatchOrganizer.AddRef(matchlist, MaleRefs);
            else
                MatchOrganizer.AddRef(matchlist, FemaleRefs);

            ThisRound = matchlist;
            PlayRound();
        }

        private void SetThisRoundDoubles(List<PlayerTeam> tlist)
        {
            var matchlist = MatchOrganizer.DoublesMatchList(tlist);
            if (matchlist[0].Team1.Player1.PlayerGender == Player.Gender.Male || matchlist[0].Team1.Player2.PlayerGender == Player.Gender.Female)
                MatchOrganizer.AddRef(matchlist, MaleRefs);
            else
                MatchOrganizer.AddRef(matchlist, FemaleRefs);

            ThisRound = matchlist;
            PlayRound();
        }

        public void InitializeTournament()
        {
            Console.WriteLine("Please choose one of the following tournament types you want to simulate:");
            Console.WriteLine("1: Mens Single");
            Console.WriteLine("2: Womens Single");
            Console.WriteLine("3: Mens Double");
            Console.WriteLine("4: Womens Double");
            Console.WriteLine("5: Mix Double");
            string index = Console.ReadLine();
            if (index == "1")
                SetThisRoundSingles(MenSingles);
            else if (index == "2")
                SetThisRoundSingles(WomenSingles);
            else if (index == "3")
                SetThisRoundDoubles(MenDoubles);
            else if (index == "4")
                SetThisRoundDoubles(WomenDoubles);
            else if (index == "5")
                SetThisRoundDoubles(MixDoubles);
            else
                Console.WriteLine("Please insert valid number");
        }

        private void PlayRound()
        {
            NoRounds += 1;
            if (ThisRound.Count == 1) Console.WriteLine("Welcome to the final!");
            else Console.WriteLine("Welcome to round number " + NoRounds + "!");
            Random rand = new Random();
            foreach (Match p in ThisRound)
            {
                p.PlayMatch(rand);
            }
            if (ThisRound.Count > 1)
            {
                var nextround = MatchOrganizer.NextRound(ThisRound);
                ThisRound = nextround;
                Console.WriteLine("");
                Console.WriteLine("Press any key to start next round:");
                Console.ReadKey();

                PlayRound();
            }
            else
            {

                if (ThisRound[0].WinnerSingle == null)
                {

                    Console.WriteLine(ThisRound[0].WinnerDouble.Player1.FirstName + " " +
                        ThisRound[0].WinnerDouble.Player1.LastName + " and " +
                        ThisRound[0].WinnerDouble.Player2.FirstName + " " +
                        ThisRound[0].WinnerDouble.Player2.LastName + " wins the tournament!");
                }
                else
                    Console.WriteLine(ThisRound[0].WinnerSingle.FirstName + " " + ThisRound[0].WinnerSingle.LastName + " wins the tournament!");
            }
        }
    }

    public abstract class Person
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateofBirth { get; set; }
        public Gender PlayerGender { get; set; }
        public string Country { get; set; }
        public string ShortNameCountry { get; set; }


        public enum Gender { Male, Female }
        
        protected Person(int id, string fn, string mn, string ln, DateTime dob,
                      string gen, string cou, string snc)
        {
            ID = id;
            FirstName = fn;
            MiddleName = mn;
            LastName = ln;
            DateofBirth = dob;
            if (gen == "Male") { PlayerGender = Gender.Male; }
            else { PlayerGender = Gender.Female; }
            Country = cou;
            ShortNameCountry = snc;
        }
    }

    public class Player : Person
    {

        public Player (int id, string fn, string mn, string ln, DateTime dob,
                       string gen, string cou, string snc) : base(id ,fn, mn, ln, dob,
                       gen, cou, snc) { }


        public string GetAge()
        {
            int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            int dob = int.Parse(DateofBirth.ToString("yyyyMMdd"));
            int age = (now - dob) / 10000;
            string returnstring = "The age of " + FirstName + " " + MiddleName + " " + LastName + " is " + age + ".";
            return returnstring;
        }
    }

    public class Referee : Person
    {
        public DateTime LicenseGot { get; set; }
        public DateTime LicenseRenewal { get; set; }

        public Referee(int id, string fn, string mn, string ln, DateTime dob, string gen,
            string cou, string snc, DateTime lg, DateTime lr) : base(id, fn, mn, ln, dob, gen, cou, snc)
        {
            LicenseGot = lg;
            LicenseRenewal = lr;
        }

    }
    public class PlayerTeam
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public TypeofTeam TeamType { get; set; }


        public enum TypeofTeam { MensDouble, WomensDouble, MixDouble }

        public PlayerTeam(Player p1, Player p2)
        {
            Player1 = p1;
            Player2 = p2;
            if (p1.PlayerGender == Player.Gender.Male && p2.PlayerGender == Player.Gender.Male)
                TeamType = TypeofTeam.MensDouble;
            else if (p1.PlayerGender == Player.Gender.Female && p2.PlayerGender == Player.Gender.Female)
                TeamType = TypeofTeam.WomensDouble;
            else
                TeamType = TypeofTeam.MixDouble;

        }

    }
    public static class Readfile
    {
        public static List<Player> GetPlayers(string fn, string gender, Encoding encoding)
        {
            string line;
            List<Player> listOfPersons = new List<Player>();
            System.IO.StreamReader file =
                new System.IO.StreamReader(fn, encoding);

            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split('|');
                DateTime stringtodate = DateTime.ParseExact(words[4], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                listOfPersons.Add(new Player(Convert.ToInt32(words[0]), words[1], words[2],
                                                             words[3], stringtodate, gender,
                                                             words[5], words[6]));


            }
            file.Close();
            return listOfPersons;
        }
        public static List<Referee> GetRefs(string fn, string gender, Encoding encoding)
        {
            string line;
            List<Referee> listOfPersons = new List<Referee>();
            System.IO.StreamReader file =
                new System.IO.StreamReader(fn, encoding);

            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split('|');
                DateTime stringtodate1 = DateTime.ParseExact(words[4], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime stringtodate2 = DateTime.ParseExact(words[7], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime stringtodate3 = DateTime.ParseExact(words[8], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                listOfPersons.Add(new Referee(Convert.ToInt32(words[0]), words[1], words[2],
                                                             words[3], stringtodate1, gender,
                                                             words[5], words[6], stringtodate2, stringtodate3));


            }
            file.Close();
            return listOfPersons;
        }

        public static List<PlayerTeam> MakeSameGenderTeams(List<Player> p, int noteams)
        {
            List<PlayerTeam> teamlist = new List<PlayerTeam>();
            for (int i = 0; i < noteams; i += 2)
            {
                teamlist.Add(new PlayerTeam(p[i], p[i + 1]));
            }
            return teamlist;
        }

        public static List<PlayerTeam> MakeMixedGenderTeams(List<Player> men, List<Player> women, int noteams)
        {
            List<PlayerTeam> mixdoubles = new List<PlayerTeam>();
            for (int i = 0; i < noteams; i++)
            {
                mixdoubles.Add(new PlayerTeam(men[i], women[i]));
            }
            return mixdoubles;
        }

    }
    

    public static class MatchOrganizer
    {
        public static List<Match> SinglesMatchList(List<Player> plist)
        {
            List<Match> matchlist = new List<Match>();
            int x, y;
            x = 0; y = 1;
            for (int i = 0; i < 16; i++)
            {
                matchlist.Add(new Match(plist[x], plist[y]));

                x += 2;
                y += 2;
            }

            return matchlist;
        }

        public static List<Match> DoublesMatchList(List<PlayerTeam> ptlist)
        {
            List<Match> matchlist = new List<Match>();
            int x, y;
            x = 0; y = 1;
            for (int i = 0; i < 16; i++)
            {
                matchlist.Add(new Match(ptlist[x], ptlist[y]));

                x += 2;
                y += 2;
            }

            return matchlist;

        }

        public static List<Match> NextRound(List<Match> winners)
        {
            List<Match> nextround = new List<Match>();
            int i = 0;
            if (winners[0].WinnerDouble == null)
            {
                while (nextround.Count < winners.Count / 2)
                {
                    nextround.Add(new Match(winners[i].WinnerSingle, 
                                            winners[i + 1].WinnerSingle));
                    i += 2;
                }
            }
            else
            {
                while (nextround.Count < winners.Count / 2)
                {
                    nextround.Add(new Match(winners[i].WinnerDouble, 
                                            winners[i + 1].WinnerDouble));
                    i += 2;
                }
            }

            return nextround;
        }

        public static List<Match> AddRef(List<Match> mlist, List<Referee> rlist)
        {
            int i = 0;
            foreach (Match m in mlist)
            {
                m.Referee = rlist[i];
                i++;
            }
            return mlist;
        }
    }

    public class Match 
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public PlayerTeam Team1 { get; set; }
        public PlayerTeam Team2 { get; set; }
        public Referee Referee { get; set; }
        public int NoSets { get; set; }
        public int SetsToWin { get; set; }
        public int SetsWon1 { get; set; }
        public int SetsWon2 { get; set; }
        public int Points1 { get; set; }
        public int Points2 { get; set; }
        public GameType TypeofGame { get; set; }
        public Player WinnerSingle { get; set; }
        public PlayerTeam WinnerDouble { get; set; }
        public const int PointsMax = 6;


        public enum GameType { Mensingle, WomenSingle, MenDouble, WomenDouble, Mixdouble }
        public Match()
        {
            SetsWon1 = 0;
            SetsWon2 = 0;
            Points1 = 0;
            Points2 = 0;
        }

        public Match(Player p1, Player p2) : this()
        {
            Player1 = p1;
            Player2 = p2;
            if (p1.PlayerGender == Player.Gender.Male)
            {
                NoSets = 5;
                TypeofGame = GameType.Mensingle;
                SetsToWin = 3;
            }
            else
            {
                NoSets = 3;
                TypeofGame = GameType.WomenSingle;
                SetsToWin = 2;
            }
        }

        public Match(PlayerTeam pt1, PlayerTeam pt2) : this()
        {
            Team1 = pt1;
            Team2 = pt2;
            if (pt1.TeamType == PlayerTeam.TypeofTeam.WomensDouble)
            {
                NoSets = 3;
                TypeofGame = GameType.WomenDouble;
                SetsToWin = 2;
            }
            else if (pt1.TeamType == PlayerTeam.TypeofTeam.MensDouble)
            {
                NoSets = 5;
                TypeofGame = GameType.MenDouble;
                SetsToWin = 3;
            }
            else
            {
                NoSets = 5;
                TypeofGame = GameType.Mixdouble;
                SetsToWin = 3;
            }
        }

        internal void PlayMatch(Random rand)
        {
            if (Player1 == null)
            {
                Console.WriteLine("This match is between:");
                Console.WriteLine("Home: " + Team1.Player1.FirstName + " " +
                                             Team1.Player1.MiddleName + " " +
                                             Team1.Player1.LastName + " and " +
                                             Team1.Player2.FirstName + " " +
                                             Team1.Player2.MiddleName + " " +
                                             Team1.Player2.LastName);
                Console.WriteLine("VS");
                Console.WriteLine("Away: " + Team2.Player1.FirstName + " " +
                                             Team2.Player1.MiddleName + " " +
                                             Team2.Player1.LastName + " and " +
                                             Team2.Player2.FirstName + " " +
                                             Team2.Player2.MiddleName + " " +
                                             Team2.Player2.LastName);
            }
            else
            {
                Console.WriteLine("This match is between: ");
                Console.WriteLine("Home: " + Player1.FirstName + " " +
                                             Player1.MiddleName + " " +
                                             Player1.LastName);
                Console.WriteLine("VS");
                Console.WriteLine("Away: " + Player2.FirstName + " " +
                                             Player2.MiddleName + " " +
                                             Player2.LastName);
            }

            while (SetsWon1 < SetsToWin && SetsWon2 < SetsToWin)
            {
                Points1 = 0;
                Points2 = 0;
                int points = 0;

                while (Points1 < PointsMax && Points2 < PointsMax)
                {

                    points = rand.Next(1, 4);
                    if (points == 1)
                    {
                        Points1++;
                    }
                    else if (points == 2)
                    {
                        Points2++;
                    }
                    if (Points1 == PointsMax)
                    {
                        SetsWon1++;
                        Console.WriteLine("Home wins set: " + Points1 + "-" + Points2);
                    }
                    else if (Points2 == PointsMax)
                    {
                        SetsWon2++;
                        Console.WriteLine("Away wins set: " + Points1 + "-" + Points2);
                    }
                }


                if (TypeofGame == GameType.Mensingle || TypeofGame == GameType.WomenSingle)
                {
                    if (SetsWon1 == SetsToWin)
                        WinnerSingle = Player1;
                    else if (SetsWon2 == SetsToWin)
                        WinnerSingle = Player2;
                }
                else if (TypeofGame == GameType.MenDouble || TypeofGame == GameType.WomenDouble || TypeofGame == GameType.Mixdouble)
                {
                    if (SetsWon1 == SetsToWin)
                        WinnerDouble = Team1;
                    else if (SetsWon2 == SetsToWin)
                        WinnerDouble = Team2;
                }

            }
            if (WinnerSingle == null)
                Console.WriteLine("The winners of the match is: " + WinnerDouble.Player1.FirstName + " " +
                                                                    WinnerDouble.Player1.MiddleName + " " +
                                                                    WinnerDouble.Player1.LastName + " and " +
                                                                    WinnerDouble.Player2.FirstName + " " +
                                                                    WinnerDouble.Player2.MiddleName + " " +
                                                                    WinnerDouble.Player2.LastName + ": " + SetsWon1 + "-" + SetsWon2 + "!");
            else
                Console.WriteLine("The winner of the match is: " + WinnerSingle.FirstName + " " +
                                                                   WinnerSingle.MiddleName + " " +
                                                                   WinnerSingle.LastName + ": " + SetsWon1 + "-" + SetsWon2 + "!");
            Console.WriteLine("");
        }
    }
}