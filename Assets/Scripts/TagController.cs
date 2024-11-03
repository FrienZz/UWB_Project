using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.WebRequestMethods;

public class TagController : MonoBehaviour
{
    private string URL = "https://670d5835073307b4ee433e78.mockapi.io/anchor";

    public TextMeshProUGUI RangeA1;
    public TextMeshProUGUI RangeA2;
    public TextMeshProUGUI RangeA3;
    public TextMeshProUGUI RangeA4;
    public TextMeshProUGUI PositionTag ;
    //[SerializeField] float repeatTime = 4f;
    //private bool canRepeat = true;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetData());
    }

    // Update is called once per frame
    void Update()
    {
        /**
        if (canRepeat)
        {
            StartCoroutine(GetData());
        }
        **/
    }

    IEnumerator GetData()
    {

        float[] distance = new float[4];
  

        using (UnityWebRequest request = UnityWebRequest.Get(URL))
        {
            yield return request.SendWebRequest();

           
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.LogError(request.error);
            else
            {
                string json = request.downloadHandler.text;
                SimpleJSON.JSONNode range = SimpleJSON.JSON.Parse(json);

                RangeA1.text = "A1 : " + range[0]["range"] + "  mm";
                RangeA2.text = "A2 : " + range[1]["range"] + "  mm"; 
                RangeA3.text = "A3 : " + range[2]["range"] + "  mm";
                RangeA4.text = "A4 : " + range[3]["range"] + "  mm";

                for (int i = 0;i < 4; i++)
                {
                    distance[i] = range[i]["range"]; 
                }
            }

        }

        // {x, y, z, r} units:milimeter
        List <float[]> position_anchor = new List<float []> { 
            new float[4] { 519.618f, 876.596f, -51.963f, distance[0] }, // Anchor1 : x1,y1,z1,r1
            new float[4] { 19.6184f, 10.5662f, -51.963f, distance[1] }, // Anchor2 : x2,y2,z2,r2
            new float[4] { 1019.62f, 10.5648f, -51.963f, distance[2] }, // Anchor3 : x3,y3,z3,r3
            new float[4] { 519.618f, 299.242f, 764.531f, distance[3] }  // Anchor4 : x4,y4,z4,r4
        };

        float[,] position = CalculatePosition(position_anchor[0], position_anchor[1], position_anchor[2], position_anchor[3]);

        //Round to 3 decimal places
        float positionX = Convert.ToSingle(Math.Round(position[0, 0], 3));
        float positionY = Convert.ToSingle(Math.Round(position[1, 0], 3));
        float positionZ = Convert.ToSingle(Math.Round(position[2, 0], 3));

        PositionTag.text = "X : " + positionX + "  mm\n\n" + "Y : " + positionY + "  mm\n\n" + "Z : " + positionZ + "  mm";

        /*
        canRepeat = false;
        yield return new WaitForSeconds(repeatTime);
        canRepeat = true;
        */
    }
 

    /*
     * Parameter:
     * a1 (array): anchor1 position(x1,y1,z1,r1)
     * a2 (array): anchor2 position(x2,y2,z2,r2)  
     * a3 (array): anchor3 position(x3,y3,z3,r3)  
     * a4 (array): anchor4 position(x4,y4,z4,r4)  
     * 
     * Return:
     * 2d-array : the estimate position(x,y,z) from tag refer to all set of anchor
    */
    private float[,] CalculatePosition(float[] a1, float[] a2, float[] a3, float[] a4)
    {

        // All Set of Anchor nC3
        List<List<float[]>> all_set_anchor = new List<List<float[]>> {
            new List<float[]> { a1, a2, a3 }, // Set of Anchor 1,2,3
            new List<float[]> { a1, a2, a4 }, // Set of Anchor 1,2,4
            new List<float[]> { a1, a3, a4 }, // Set of Anchor 1,3,4
            new List<float[]> { a2, a3, a4 }  // Set of Anchor 2,3,4
        };
        
        float[,] initial_guess = new float[3, 1] { { 0 }, { 0 }, { 0 } };
        List < float[,]> set_of_position = new List<float[,]>();

        for(int i = 0; i < 4; i++)
        {
            float[,] inv_matrix = InverseJacobianMatrix3x3(all_set_anchor.ElementAt(i), initial_guess);
            float[,] initial_func_matrix = FindFunctionValue(all_set_anchor.ElementAt(i), initial_guess);
            set_of_position.Add(NewtonRapsonMethod(inv_matrix, initial_func_matrix, all_set_anchor.ElementAt(i), initial_guess));
        }

        /*
        for(int i = 0;i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Debug.Log(Convert.ToSingle(Math.Round(set_of_position.ElementAt(i)[j, 0], 3)));
            }
            Debug.Log("\n");
        }
        */
        

        float x_mean = 0;
        float y_mean = 0;
        float z_mean = 0;
        for (int i = 0;i < 4; i++)
        {
            x_mean += set_of_position.ElementAt(i)[0, 0] / 4;
            y_mean += set_of_position.ElementAt(i)[1, 0] / 4;
            z_mean += set_of_position.ElementAt(i)[2, 0] / 4;
        }

        float[,] final_result = new float[3, 1] { { x_mean } , { y_mean }, {z_mean } };


        return final_result;
    }

    /*
     * Parameter:
     * inv_matrix (2d-array): inverse of matrix size3*3
     * initial_func_matrix (2d-array): matrix size3*1 with initial guess value
     * set_anchor (List of array): each set of anchor in C(4,3)
     * initial_guess (2d-array): initial guess value of matrix size3*1   
     * 
     * Return:
     * 2d-array : the estimate position(x,y,z) from tag refer to specific set of anchor
    */
    private float[,] NewtonRapsonMethod(float [,] inv_matrix , float[,] initial_func_matrix, List<float[]> set_anchor,float [,] initial_guess)
    {

        float[,] result = new float[3, 1];
        float[,] current_matrix = inv_matrix;
        float[,] current_func = initial_func_matrix;
        float[,] current_round = initial_guess;
        float[,] multiply_matrix = Matrix3x3_multiplication(inv_matrix, current_func);

        for (int i = 0; i < 7; i++) 
        {
            for (int j = 0; j < 3; j++)
            {
                result[j, 0] = current_round[j, 0] - multiply_matrix[j, 0];
                //Debug.Log(result[j, 0]);
            }
            current_round = result;
            current_matrix = InverseJacobianMatrix3x3(set_anchor, current_round);
            current_func = FindFunctionValue(set_anchor, result);
            multiply_matrix = Matrix3x3_multiplication(current_matrix, current_func);
        }

        return result;
    }

    /*
     * Parameter:
     * set_anchor (List of array) : each set of anchor in C(4,3)
     * initial_guess (2d-array): initial guess value of matrix size3*1 
     * 
     * Return:
     * 2d-array : function value of matrix size3*1
    */
    private float[,] FindFunctionValue(List<float[]> set_anchor,float[,] initial_guess)
    {
        float[,] func_val = new float[3, 1];

        for (int i = 0;i < 3; i++)
        {
            func_val[i, 0] = Convert.ToSingle(Math.Pow(initial_guess[0, 0] - set_anchor.ElementAt(i)[0], 2) + Math.Pow(initial_guess[1, 0] - set_anchor.ElementAt(i)[1], 2) + Math.Pow(initial_guess[2, 0] - set_anchor.ElementAt(i)[2], 2) - Math.Pow(set_anchor.ElementAt(i)[3],2));
        }

        return func_val ;
    }

    /*
     * Parameter:
     * inv_matrix (2d-array) : inverse of matrix size3*3 
     * func_matrix (2d-array): function of matrix size3*1 
     * 
     * Return:
     * 2d-array : multiplication of matrix size3*3
    */
    private float[,] Matrix3x3_multiplication(float[,] inv_matrix, float[,] func_matrix)
    {
        float[,] multi_matrix = new float[3, 1];

        for(int i = 0; i < 3; i++)
        {
            multi_matrix[i,0] = inv_matrix[i, 0] * func_matrix[0, 0] + inv_matrix[i, 1] * func_matrix[1, 0] + inv_matrix[i, 2] * func_matrix[2, 0];
        }

        return multi_matrix ;
    }

    /*
     * Parameter:
     * matrix (2d-array) : matrix size3*3 
     * 
     * Return:
     * 2d-array : determinant of matrix size3*3
    */
    private float DeterminantMatrix3x3(float[,] matrix)
    {

        float det = 0;
        for(int i = 0;i < 3; i++)
        {
            det += (matrix[0, i] * matrix[1, (i + 1) % 3] * matrix[2, (i + 2) % 3] - matrix[2,i] * matrix[1,(i + 1) % 3] * matrix[0,(i + 2) % 3]);
        }

        return det ;
    }

   /*
    * Parameter:
    * matrix (2d-array) : matrix size2*2 
    * 
    * Return:
    * 2d-array : determinant of matrix size2*2
   */
    private float DeterminantMatrix2x2(float[,] matrix)
    {

        float det = 0;
        det = matrix[0, 0] * matrix[1, 1] - matrix[1, 0] * matrix[0,1];

        return det;
    }

    /*
     * Parameter:
     * matrix (2d-array) : matrix size3*3 
     * row (int) : remove specific row of matrix size3*3
     * col (int) : remove specific column of matrix size3*3
     * 
     * Return:
     * 2d-array : submatrix size2*2
    */
    private float[,] SubMatrix3x3(float[,] matrix , int row , int col)
    {

        float[,] submatrix = new float[2, 2];
        for(int i = 0;i < 3; i++)
        {
            for(int j = 0;i != row && j < 3; j++)
            {
                if(j != col)
                {
                    submatrix[i < row ? i : i - 1,j < col ? j : j-1] = matrix[i,j];
                }
            }
        } 

        return submatrix;
    }

    /*
     * Parameter:
     * matrix (2d-array) : matrix size3*3 
     * 
     * Return:
     * 2d-array : transpose of matrix size3*3
    */
    private float[,] TransposeMatrix3x3(float[,] matrix)
    {
        float[,] trans_matrix = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                trans_matrix[j, i] = matrix[i, j];
            }
        }

        return trans_matrix;
    }


    /*
     * Parameter:
     * input (List of array) :  all possible set of anchor in C(4,3) 
     * initial_guess (2d-array) : initial guess value of matrix size3*1
     * 
     * Return:
     * 2d-array : inverse of partial_matrix size3*3
    */
    private float[,] InverseJacobianMatrix3x3(List<float[]> input, float[,] initial_guess)
    {
        float[,] jaco_matrix = new float[3, 3];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                jaco_matrix[i, j] = 2 * (initial_guess[j,0] - input.ElementAt(i)[j]);
            }
            
        }

        float[,] trans_matrix = TransposeMatrix3x3(jaco_matrix);
        float[,] inv_jaco_matrix = InverseMatrix3x3(trans_matrix);


        return inv_jaco_matrix;
    }

    /*
     * Parameter:
     * t_matrix (2d-array) : transpose of matrix size3*3
     * 
     * Return:
     * 2d-array : inverse of matrix size3*3
    */
    private float[,] InverseMatrix3x3(float[,] t_matrix)
    {
        float[,] inv = new float[3, 3];

        float det = DeterminantMatrix3x3(t_matrix);
        
        if (det == 0)
        {
            throw new InvalidOperationException("Matrix is not invertible.");
        }

        //cofactor matrix2*2 for each row and column
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                inv[i, j] = Convert.ToSingle(Math.Pow(-1, i + j)) * DeterminantMatrix2x2(SubMatrix3x3(t_matrix,i,j)) / det;
            }
        }

        return inv;
    }

    

}
