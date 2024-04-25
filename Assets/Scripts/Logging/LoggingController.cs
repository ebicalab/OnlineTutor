using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using System.Text;

public class LoggingController : MonoBehaviour {

    [SerializeField] private bool enableLogging = true;
    [SerializeField] private MoralSchema moralSchema;

    [SerializeField] List<Camera> playerObjects;

    [SerializeField] GameObject teacher;

    private bool isActionsInfoLogged = false;


    private static string LOGS_FOLDER = "\\VCR_LOGS";
    private static string LOGS_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                                                        + LOGS_FOLDER;

    private static string FILENAME_EVENTS = "\\EventLogs";
    private static string FILENAME_ACTS = "\\ActionsFullInfo";

    private static int session = 1;

    private StreamWriter eventsLogs;
    private StreamWriter actionsLogs;

    private System.Diagnostics.Stopwatch gameTimer;

    // Start is called before the first frame update
    void Start()
    {
        System.IO.Directory.CreateDirectory(LOGS_PATH);

        gameTimer = new System.Diagnostics.Stopwatch();
        gameTimer.Start();

        if (!enableLogging) {
            return;
        }

        while (File.Exists(LOGS_PATH + FILENAME_EVENTS + session.ToString() + ".csv")) {
            session++;
        }
        string writePath = LOGS_PATH + FILENAME_EVENTS + session.ToString() + ".csv";

        eventsLogs = new StreamWriter(writePath, true, Encoding.GetEncoding("UTF-8"));
        using (eventsLogs) {
            eventsLogs.WriteLine(
                "Timestamp," +
                "Time from start," +
                "Action author," +
                "Action id," +
                "Action name," + 
                "Student appraisals valence," +
                "Student appraisals initiative," +
                "Student appraisals learnability," +
                "Student feelings valence," +
                "Student feelings initiative," +
                "Student feelings learnability," +
                //"Student characteristic," +
                "Coordinate X student," +
                "Coordinate Y student," +
                "Coordinate Z student," +
                "Azimuth student," +
                "Coordinate X teacher," +
                "Coordinate Y teacher," +
                "Coordinate Z teacher," +
                "Azimuth teacher,"
                );
        }

        Debug.LogError("Logger init finished");
    }

    public string vector3ToCSVString(Vector3 arr) {
        string retValue = "";
        retValue += (Math.Round(arr.x, 2)).ToString().Replace(',', '.') + ",";
        retValue += (Math.Round(arr.y, 2)).ToString().Replace(',', '.') + ",";
        retValue += (Math.Round(arr.z, 2)).ToString().Replace(',', '.');
        return retValue;
    }

    public void UpdateLogs(string author, Act action) {

        if (!enableLogging) {
            return;
        }

        if (!isActionsInfoLogged) {
            actionsLogs = new StreamWriter(LOGS_PATH + FILENAME_ACTS + ".csv", false, Encoding.GetEncoding("UTF-8"));
            using (actionsLogs) {
                string header = "id,name,nameInRussian,author,authorValence,authorInitiative,authorLearnability";
                StringBuilder sb = new StringBuilder();
                sb.Append(header).Append("\n");
                foreach (var act in moralSchema.getIndependentActions()) {
                    sb.Append($"{act.getId()},")
                       .Append($"{act.getName()},")
                       .Append($"{act.getNameInRussian()},")
                       .Append($"{act.getActionAuthor()},")
                       .Append($"{act.getMoralFactorForAuthor()[0].ToString().Replace(',', '.')},")
                       .Append($"{act.getMoralFactorForAuthor()[1].ToString().Replace(',', '.')},")
                       .Append($"{act.getMoralFactorForAuthor()[2].ToString().Replace(',', '.')},")
                       .Append("\n");
                }
                actionsLogs.WriteLine(sb.ToString());
            }
            isActionsInfoLogged = true;
        }

        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
        string[] studentAppraisals = moralSchema.getStudentAppraisals()
                                                .Select(d => Math.Round(d, 4).ToString().Replace(',', '.'))
                                                .ToArray();
        string[] studentFeelings = moralSchema.getStudentFeelings()
                                                .Select(d => Math.Round(d, 4).ToString().Replace(',', '.'))
                                                .ToArray();
        //string studentCharacteristic = moralSchema.getStudentCharacteristic();
        
        string studentPosition = vector3ToCSVString(
            playerObjects.FirstOrDefault(obj => obj.enabled == true)
                        .transform
                        .position
        );

        string teacherPosition = vector3ToCSVString(
            teacher.transform.position
        );

        string studentAzimuth = playerObjects.FirstOrDefault(obj => obj.enabled == true)
                                            .transform
                                            .rotation
                                            .eulerAngles
                                            .y
                                            .ToString()
                                            .Replace(',', '.');

        string teacherAzimuth = teacher
                                    .transform
                                    .rotation
                                    .eulerAngles
                                    .y
                                    .ToString()
                                    .Replace(',', '.');

        string writePath = LOGS_PATH + FILENAME_EVENTS + session.ToString() + ".csv";
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
        eventsLogs = new StreamWriter(writePath, true, Encoding.GetEncoding("UTF-8"));
        //Debug.LogError("action id = " + action.getId());
        using (eventsLogs) {
            eventsLogs.WriteLine(
                DateTime.Now.ToString("MM.dd.yyyy hh:mm:ss.fff") + "," +
                Math.Round(gameTimer.Elapsed.TotalSeconds, 3).ToString().Replace(',', '.') + "," +
                author + "," +
                action.getId() + "," +
                action.getNameInRussian() + "," +
                string.Join(",", studentAppraisals) + "," +
                string.Join(",", studentFeelings) + "," +
                //studentCharacteristic + "," +
                studentPosition + "," +
                studentAzimuth + "," +
                teacherPosition + "," +
                teacherAzimuth + ","

            );
        }
    }
}
