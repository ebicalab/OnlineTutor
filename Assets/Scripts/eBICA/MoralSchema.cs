using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using System.IO;

public class MoralSchema : MonoBehaviour
{

    [SerializeField] LoggingController loggingController;

    const double r = 2e-3;
    private const double r1 = 0.8;
    const double CRITICAL_VALUE_FOR_DIFF_NORMS = 0.65;

    const double FEELINGS_CIRCLE_RADIUS = 0.5D;

    private static bool IS_REALATIONS_UNSTABLE = true;

    private string JSON_PATH_ALL_ACTIONS = Application.streamingAssetsPath + "\\Actions.json";
    private string JSON_PATH_INDEPENDENT_ACTIONS = Application.streamingAssetsPath + "\\IndependentActions.json";
    private string JSON_PATH_INDEPENDENT_FEELINGS_STATES = Application.streamingAssetsPath + "\\FeelingsStates.json";

    public static int ESTIMATE_SPACE_DIMENSION = 3;

    static bool processRecoveryOfFeelings = false;
    //public static string studentCharacteristic = "NAN";

    private static bool isMoralSchemaActive = false;

    private static Dictionary<string, Act> allActs = new Dictionary<string, Act>();
    private static Dictionary<string, Act> allIndependentActions = new Dictionary<string, Act>();
    //private static Dictionary<string, FeelingState> feelingsStates = new Dictionary<string, FeelingState>();

    public List<Tuple<string, double>> biasLikelihood = new List<Tuple<string, double>>();

    static public double[] teacherAppraisals = new double[ESTIMATE_SPACE_DIMENSION];
    static public double[] studentAppraisals = new double[ESTIMATE_SPACE_DIMENSION];
    static public double[] teacherFeelings = new double[ESTIMATE_SPACE_DIMENSION];
    static public double[] studentFeelings = new double[ESTIMATE_SPACE_DIMENSION];

    private void Start() {
        setupActs();
        for (int i = 0; i < studentFeelings.Length; i++) {
            studentFeelings[i] = 0.0001D;
        }
    }

    private void Update() {

        if(Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(String.Join("\t", getStudentAppraisals().ToArray()));
            Debug.Log(String.Join("\t", getStudentFeelings().ToArray()));
        }

        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    test();
        //}
    }

    public List<Act> getIndependentActions() {
        return allIndependentActions.Values.ToList();
    }

    public class FeelingState {

        public double[] feelingState;

        FeelingState() {
            feelingState = new double[ESTIMATE_SPACE_DIMENSION];
        }

        public void setActionAuthor(double[] inputFeelingState)
        {
            inputFeelingState.CopyTo(this.feelingState, 0);
        }

        public double[] getBodyFactorForTarget()
        {
            return feelingState;
        }
    }

    public double[] getTeacherAppraisals()
    {
        return teacherAppraisals;
    }
    public double[] getTeacherFeelings()
    {
        return teacherFeelings;
    }
    public double[] getStudentAppraisals()
    {
        return studentAppraisals;
    }
    public double[] getStudentFeelings()
    {
        return studentFeelings;
    }

    //public string getStudentCharacteristic()
    //{
    //    return studentCharacteristic;
    //}


    public void setupActs() {
        //feelingsStates = JsonConvert.DeserializeObject<Dictionary<string, FeelingState>>(File.ReadAllText(JSON_PATH_INDEPENDENT_FEELINGS_STATES));
        allActs = JsonConvert.DeserializeObject<Dictionary<string, Act>>(File.ReadAllText(JSON_PATH_ALL_ACTIONS));
        allIndependentActions = JsonConvert.DeserializeObject<Dictionary<string, Act>>(File.ReadAllText(JSON_PATH_INDEPENDENT_ACTIONS));

        //foreach (var item in allIndependentActions.Values.ToArray()) {
        //    Debug.LogError(item.ToString());
        //}
    }

    public double[] recalculateAppraisals(double[] appraisals, double[] action)
    {
        //Debug.LogError("recalculate appraisals lengths: " + appraisals.Length + "\t\t" + action.Length);
        double[] resultAppraisals = new double[appraisals.Length];
        //Debug.LogError("Length - " + appraisals.Length);
        for (int i = 0; i < appraisals.Length; ++i)
        {
            //Debug.LogError("i = " + i);
            resultAppraisals[i] = (1.0 - r) * appraisals[i] + r * action[i];
        }
        return resultAppraisals;
    }

    public void makeIndependentAction(string action)
    {
        if (action == "student_takes_the_lecture_in_his_order" || action == "student_retakes_lectures") {
            Debug.LogError(" makeIndependentAction student initiative " + action);
        }
        rebuildAppraisalsAndFeelingsAfterStudentAction(action, true);
        loggingController.UpdateLogs("student", allIndependentActions[action]);
        //teacherAppraisals = recalculateAppraisals(teacherAppraisals, allIndependentActions[action].getMoralFactorForTarget());
        //studentAppraisals = recalculateAppraisals(studentAppraisals, allIndependentActions[action].getMoralFactorForAuthor());
    }

    public void makeIndependentActionByTeacher(string action) {
        rebuildAppraisalsAndFeelingsAfterTeacherAction(action, true);
        loggingController.UpdateLogs("teacher", allIndependentActions[action]);
        //teacherAppraisals = recalculateAppraisals(teacherAppraisals, allIndependentActions[action].getMoralFactorForTarget());
        //studentAppraisals = recalculateAppraisals(studentAppraisals, allIndependentActions[action].getMoralFactorForAuthor());
    }



    //string findFeelingsCharacteristic(double[] feelings)
    //{
    //    string choise = "";
    //    double dif = 20;
    //    foreach (var feelingMassive in feelingsStates)
    //    {
    //        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i)
    //        {
    //            if (feelingMassive.Value.feelingState[i] != 0)
    //            {
    //                double mid = Math.Abs(feelingMassive.Value.feelingState[i] - feelings[i]);
    //                if (mid < dif)
    //                {
    //                    dif = mid;
    //                    choise = feelingMassive.Key;
    //                }
    //            }
    //        }
    //    }
    //    Debug.LogError("student characteristic = " + choise);
    //    return choise;
    //}

    //string findFeelingsCharacteristic(double[] feelings) {
    //    //Debug.LogError("feelings = " + String.Join(",", feelings));
    //    string choise = ""; 
    //    double dif = 20;
    //    foreach (var feelingMassive in feelingsStates) {
    //        double mid = 0.0D;
    //        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i) {
    //            mid += Math.Pow(feelingMassive.Value.feelingState[i] - feelings[i], 2);
    //        }
    //        mid = Math.Sqrt(mid);
    //        if (mid < dif) {
    //            dif = mid;
    //            choise = feelingMassive.Key;
    //        }
    //        //Debug.LogError("Diff = " + mid + ", Characteristic = " + feelingMassive.Key);
    //    }
    //    //Debug.LogError("student characteristic = " + choise);
    //    return choise;
    //}


    //void isRelationsUnstable(double[] feelings)
    //{
    //    string studentCharacteristic = findFeelingsCharacteristic(feelings);
    //    double dif = 0;
    //    for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i) {
    //        dif += Math.Pow(feelingsStates[studentCharacteristic].feelingState[i] - feelings[i], 2);
    //    }
    //    dif = Math.Sqrt(dif);
    //    if (dif < 0.3) {
    //        unstableRelations = false;
    //    }
    //}

    double[] findClosestDotOnFeelingCircle(double[] feelings) {

        double[] center = new double[ESTIMATE_SPACE_DIMENSION];
        double[] deltas = new double[ESTIMATE_SPACE_DIMENSION];

        if (feelings.All(o => o == 0)) {
            var answer = new double[ESTIMATE_SPACE_DIMENSION];
            answer[0] = FEELINGS_CIRCLE_RADIUS;
            return answer;
        }

        double vectorLength = 0.0D;
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; i++) {
            var delta = feelings[i] - center[i];
            deltas[i] = delta;
            vectorLength += Math.Pow(delta, 2);
        }
        vectorLength = Math.Sqrt(vectorLength);

        var K = FEELINGS_CIRCLE_RADIUS / vectorLength;

        double[] closestDot = new double[ESTIMATE_SPACE_DIMENSION];
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; i++) {
            closestDot[i] = center[i] + deltas[i] * K;
        }

        return closestDot;
    }

    void isRelationsUnstable(double[] feelings) {
        var closestFeelingsDot = findClosestDotOnFeelingCircle(feelings);
        double dif = 0;
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i) {
            dif += Math.Pow(closestFeelingsDot[i] - feelings[i], 2);
        }
        dif = Math.Sqrt(dif);
        if (dif < 0.3) {
            IS_REALATIONS_UNSTABLE = false;
        }
    }

    double[] recalculateFeelings(double[] feelings, double[] appraisals)
    {

        if (!isMoralSchemaActive) {
            isRelationsUnstable(feelings);
            if (IS_REALATIONS_UNSTABLE) {
                feelings = firstMethodRecalculateFeelings(feelings, appraisals);
                return feelings;
            }

            feelings = findClosestDotOnFeelingCircle(feelings);
            isMoralSchemaActive = true;
            return feelings;
        }

        double diffNorm = 0;
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i) {
            diffNorm += Math.Abs(feelings[i] - appraisals[i]);
        }

        processRecoveryOfFeelings = diffNorm > CRITICAL_VALUE_FOR_DIFF_NORMS;

        if (processRecoveryOfFeelings) {
            feelings = secondMethodRecalculateFeelings(feelings, appraisals);
        } else {
            feelings = findClosestDotOnFeelingCircle(feelings);
        }

        return feelings;
    }

    double[] firstMethodRecalculateFeelings(double[] feelings, double[] appraisals) {
        //Debug.LogError("firstMethodRecalculateFeelings");
        double[] resultFeelings = new double[feelings.Length];
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; i++)
        {
            resultFeelings[i] = 1.1 * appraisals[i];
        }
        return resultFeelings;
    }

    double[] secondMethodRecalculateFeelings(double[] feelings, double[] appraisals)
    {
        //Debug.LogError("secondMethodRebuildFeelings");
        double[] resultFeelings = new double[feelings.Length];
        for (int i = 0; i < ESTIMATE_SPACE_DIMENSION; ++i)
        {
            resultFeelings[i] = (1 - r1) * feelings[i] + r1 * (appraisals[i] - feelings[i]);
        }
        //studentCharacteristic = "NAN";
        return resultFeelings;
    }

    //double[] setConstantFeelings(double[] feelings)
    //{
    //    //Debug.LogError("setConstantFeelings");
    //    studentCharacteristic = findFeelingsCharacteristic(feelings);
    //    double[] ans = new double[ESTIMATE_SPACE_DIMENSION];
    //    feelingsStates[studentCharacteristic].feelingState.CopyTo(ans, 0);
    //    return ans;
    //}

    public void biasCriterion(double[] appraisalsFactor, double[] feelingsFactor, string action)
    {

        double maxValue = 0;
        double mainNorm = 0;
        double recNorm = 0;

        foreach (var el in allActs)
        {
            double difference = 0;
            var responseAction = el.Value;
            
            if (responseAction.getResponseActionOn() == action)
            {
                var appraisalsAfterAction = recalculateAppraisals(appraisalsFactor, responseAction.getMoralFactorForTarget());
                
                for (int i = 0; i < feelingsFactor.Length; ++i)
                {
                    difference += Math.Pow(feelingsFactor[i] - appraisalsAfterAction[i], 2);
                }
                
                biasLikelihood.Add(new Tuple<string, double>(responseAction.getName(), difference));
                mainNorm += difference;
                maxValue = Math.Max(maxValue, difference);
            }
        }
        for (int i = 0; i < biasLikelihood.Count; ++i)
        {
            biasLikelihood[i] = new Tuple<string, double>(biasLikelihood[i].Item1, 1 - biasLikelihood[i].Item2 / mainNorm);
            recNorm += biasLikelihood[i].Item2;
            // biasLikelihood[i] = new Tuple<string, double>(biasLikelihood[i].Item1, maxValue - biasLikelihood[i].Item2);
        }
        for (int i = 0; i < biasLikelihood.Count; ++i)
        {
            biasLikelihood[i] = new Tuple<string, double>(biasLikelihood[i].Item1, biasLikelihood[i].Item2 / recNorm);
            Debug.Log("likelihood for " + biasLikelihood[i].Item1 + " " + biasLikelihood[i].Item2);
        }
    }

    void rebuildAppraisalsAndFeelingsAfterStudentAction(string studentAction, bool independent = false)
    {
        if (independent == true) {
            studentAppraisals = recalculateAppraisals(studentAppraisals, allIndependentActions[studentAction].getMoralFactorForAuthor());
            studentFeelings = recalculateFeelings(studentFeelings, studentAppraisals);

            teacherAppraisals = recalculateAppraisals(teacherAppraisals, allIndependentActions[studentAction].getMoralFactorForTarget());
            //teacherFeelings = recalculateFeelings(teacherFeelings, teacherAppraisals);
        } else {
            studentAppraisals = recalculateAppraisals(studentAppraisals, allActs[studentAction].getMoralFactorForAuthor());
            studentFeelings = recalculateFeelings(studentFeelings, studentAppraisals);

            teacherAppraisals = recalculateAppraisals(teacherAppraisals, allActs[studentAction].getMoralFactorForTarget());
            //teacherFeelings = recalculateFeelings(teacherFeelings, teacherAppraisals);
        }
        limitAppraisalsAndFeelings();
    }

    void rebuildAppraisalsAndFeelingsAfterTeacherAction(string teacherAction, bool independent = false)
    {
        if(independent == true)
        {
            studentAppraisals = recalculateAppraisals(studentAppraisals, allIndependentActions[teacherAction].getMoralFactorForTarget());
            studentFeelings = recalculateFeelings(studentFeelings, studentAppraisals);

            teacherAppraisals = recalculateAppraisals(teacherAppraisals, allIndependentActions[teacherAction].getMoralFactorForAuthor());
            //teacherFeelings = recalculateFeelings(teacherFeelings, teacherAppraisals);
        }
        else
        {
            studentAppraisals = recalculateAppraisals(studentAppraisals, allActs[teacherAction].getMoralFactorForTarget());
            studentFeelings = recalculateFeelings(studentFeelings, studentAppraisals);

            teacherAppraisals = recalculateAppraisals(teacherAppraisals, allActs[teacherAction].getMoralFactorForAuthor());
            //teacherFeelings = recalculateFeelings(teacherFeelings, teacherAppraisals);
        }

        limitAppraisalsAndFeelings();
    }

    // ¬озвращает действие с наибольшей веро¤тностью
    public string getResponseAction()
    {
        string response = "";
        List<Tuple<string, double>> result = new List<Tuple<string, double>>();
        foreach (var biasEl in biasLikelihood)
        {
            double likelihood = biasEl.Item2;
            result.Add(new Tuple<string, double>(biasEl.Item1, likelihood));
        }
        double actionLikelihood = 0;
        foreach (var el in result)
        {
            if (el.Item2 > actionLikelihood)
            {
                //Debug.Log($"{el.Item1} - {el.Item2}");
                actionLikelihood = el.Item2;
                response = el.Item1;
            }
        }
        return response;
    }

    // ¬озвращает действие в соответствии с их веро¤тност¤ми.
    public string getResponseActionByLikelihood()
    {
        string response = "";
        List<Tuple<string, double>> result = new List<Tuple<string, double>>();
        double sum = 0;
        foreach (var biasEl in biasLikelihood)
        {
            double likelihood = biasEl.Item2;
            result.Add(new Tuple<string, double>(biasEl.Item1, likelihood));
            sum += likelihood;
        }
        for (int i = 0; i < result.Count; ++i)
        {
            result[i] = new Tuple<string, double>(result[i].Item1, result[i].Item2 / sum);
        }
        result.Sort((x1, y1) => x1.Item2.CompareTo(y1.Item2));
        System.Random x = new System.Random();
        double actionLikelihood = Convert.ToDouble(x.Next(0, 10000) / 10000.0);
        double currentLikelihood = 0;
        foreach (var el in result)
        {
            currentLikelihood += el.Item2;
            if (currentLikelihood > actionLikelihood)
            {
                response = el.Item1;
                break;
            }
        }
        return response;
    }

    // ”становление лимита дл¤ векторов VAD от -0.5 до 0.5
    double[] limitVectorsOfPAD(double[] vec)
    {
        for (int i = 0; i < vec.Length; ++i)
        {
            vec[i] = Math.Min(vec[i], 0.5);
            vec[i] = Math.Max(vec[i], -0.5);
        }
        return vec;
    }

    void limitAppraisalsAndFeelings()
    {
        limitVectorsOfPAD(studentAppraisals).CopyTo(studentAppraisals, 0);
        limitVectorsOfPAD(studentFeelings).CopyTo(studentFeelings, 0);
        limitVectorsOfPAD(teacherAppraisals).CopyTo(teacherAppraisals, 0);
        limitVectorsOfPAD(teacherFeelings).CopyTo(teacherFeelings, 0);
    }

    public string getResponseAction(string studentAction)
    {
        biasLikelihood = new List<Tuple<string, double>>();

        rebuildAppraisalsAndFeelingsAfterStudentAction(studentAction);
        biasCriterion(studentAppraisals, studentFeelings, studentAction);
        string answer = getResponseActionByLikelihood();
        rebuildAppraisalsAndFeelingsAfterTeacherAction(answer);

        return answer;
    }

    public string getResponseActionWithoutRecalculateAfterStudentAction(string studentAction)
    {
        biasLikelihood = new List<Tuple<string, double>>();
        biasCriterion(studentAppraisals, studentFeelings, studentAction);
        string answer = getResponseAction();
        //string answer = getResponseActionByLikelihood();
        rebuildAppraisalsAndFeelingsAfterTeacherAction(answer);
        return answer;
    }

}

public class Act {
    [JsonRequired]
    private int id;

    [JsonRequired]
    public double[] moralFactorForTarget;

    [JsonRequired]
    public double[] moralFactorForAuthor;

    [JsonRequired]
    public string name;

    [JsonRequired]
    public string nameInRussian;

    [JsonRequired]
    public string responseActionOn;

    [JsonRequired]
    public string actionAuthor;

    public Act() {
        moralFactorForTarget = new double[MoralSchema.ESTIMATE_SPACE_DIMENSION];
        moralFactorForAuthor = new double[MoralSchema.ESTIMATE_SPACE_DIMENSION];
    }

    public void setMoralFactorForTarget(double[] values) {
        values.CopyTo(moralFactorForTarget, 0);
    }
    public void setMoralFactorForAuthor(double[] values) {
        values.CopyTo(moralFactorForAuthor, 0);
    }

    public void setName(string name) {
        this.name = name;
    }
    public void setResponseActionOn(string responseActionOn) {
        this.responseActionOn = responseActionOn;
    }

    public void setActionAuthor(string actionAuthor) {
        this.actionAuthor = actionAuthor;
    }
    public double[] getMoralFactorForTarget() {
        return moralFactorForTarget;
    }

    public double[] getMoralFactorForAuthor() {
        return moralFactorForAuthor;
    }

    public string getName() {
        return name;
    }

    public string getResponseActionOn() {
        return responseActionOn;
    }

    public string getActionAuthor() {
        return actionAuthor;
    }

    public int getId() {
        return id;
    }

    public string getNameInRussian() {
        return nameInRussian;
    }

    public override string ToString() {
        return "{" +
            "id=" + id + ", " +
            "nameInRussian=" + nameInRussian + ", " +
            "moralFactorForTarget=" + "[" + String.Join(", ", moralFactorForTarget) + "]" +
            "moralFactorForAuthor=" + "[" + String.Join(", ", moralFactorForAuthor) + "]" + "}";
    }

}