using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour
{
    const int MAX = 1000;

    public int lines = 0, colums = 0;
    public float squareLenght = 1;
    public GameObject square;
    public GameObject mainSquare;
    private GameObject[,] Square = new GameObject[MAX,MAX];
    private bool[,] A = new bool[MAX, MAX];
    private int[,] Dist = new int[MAX, MAX];
    public int distance = 5;

    struct pos{
        public int x, y;
    };

    private Queue<pos> Q = new Queue<pos>();
    private int[] dx = {0, 1, 0, -1};
    private int[] dy = {1, 0, -1, 0};
    
    void Start()
    {
        square.transform.localScale = new Vector3(1, 1, 1) * squareLenght;
        mainSquare.transform.localScale = new Vector3(colums, lines, 1) * squareLenght + new Vector3(.1f, .1f, 0f);

        int i, j;
        for( i=0; i<lines; i++ )
            for( j=0; j<colums; j++ ){
                Vector2 position = new Vector2(0f, 0f);
                position.y = (float)lines / 2 - i - 0.5f;
                position.x = (float)colums / 2 - j - 0.5f;
                position *= squareLenght;
                Square[i, j] = Instantiate(square, position, Quaternion.identity, gameObject.transform);
            }
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButtonDown(0) ) Click();
        if( Input.GetKeyDown(KeyCode.R) ) ResetMatrix();
        if( Input.GetKeyDown(KeyCode.Return) ) CalculateMatrix();
        ColorMatrix();
    }

    void ColorMatrix(){
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                if( A[i, j] == false ) Square[i,j].GetComponent<SpriteRenderer>().color = Color.black;
                else Square[i, j].GetComponent<SpriteRenderer>().color = Color.white * (float)((float)(2*distance - 1 - Dist[i, j] + 1) / (float)(2*distance - 1));
    }

    void ResetMatrix(){
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                A[i, j] = false;
    }

    void Click(){
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 boundingBox = (mainSquare.transform.localScale - new Vector3(.1f, .1f, 0f)) / 2f;
        if( mousePos.y < -boundingBox.y || mousePos.y > boundingBox.y || mousePos.x < -boundingBox.x || mousePos.x > boundingBox.x ) return;

        int i = lines - (int)((mousePos.y + boundingBox.y) / squareLenght) - 1;
        int j = colums - (int)((mousePos.x + boundingBox.x) / squareLenght) - 1;

        A[i, j] = !A[i, j];
    }

    bool InMatrix( int x, int y ){
        return !(x < 0 || x >= lines || y < 0 || y >= colums);
    }

    int NumberOfNeighbours( int x, int y ){
        int counter = 0;
        for( int k=0; k<4; k++ ){
            int xc = x + dx[k];
            int yc = y + dy[k];
            if(!InMatrix(xc, yc)) continue;
            if( A[xc, yc] == true ) counter++;
        }
        return counter;
    }

    bool Probability( float probability ){
        float chance = Random.Range(0f, 100f);
        return (probability >= chance);
    }

    void AddNeighboursInQueue( int i, int j, int d ){
        for( int k=0; k<4; k++ ){
            int x = i + dx[k];
            int y = j + dy[k];
            if(!InMatrix(x, y) || A[x, y] == true || Dist[x, y] > 0) continue;
            Dist[x, y] = d;
            pos In;
            In.x = x; In.y = y;
            Q.Enqueue(In);
        }
    }

    float f( float x ){
        return 1/Mathf.Pow(x, 0.25f);
    }

    void CalculateMatrix(){
        int i,j;
        for( i=0; i<lines; i++ )
            for( j=0; j<colums; j++ )
                Dist[i, j] = 0;
        Q.Clear();

        for( i=0; i<lines; i++ )
            for( j=0; j<colums; j++ )
                if( A[i, j] == true ){
                    Dist[i, j] = 1;
                    AddNeighboursInQueue(i, j, 2);
                }

        // Premature Quit
        if( Q.Count == 0 ) return;

        int d;
        pos P = Q.Dequeue();
        for( d=2; d<=distance + 1; d++ ){
            while(Dist[P.x, P.y] == d){
                int numberOfNeighbours = NumberOfNeighbours(P.x, P.y);
                float probability = numberOfNeighbours * 50f * f(d - 1);
                if( numberOfNeighbours == 4 ) probability = 100f;
                if( Probability(probability) ){
                    A[P.x, P.y] = true;
                    AddNeighboursInQueue(P.x, P.y, d + 1);
                }
                if( Q.Count != 0 ) P = Q.Dequeue();
                else break;
            }
        }
    }
}
