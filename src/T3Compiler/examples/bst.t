tint main() {
    tint A[9]; tint B[9]; tint C[9];
    A[0]=1;A[1]=2;A[2]=3;A[3]=4;A[4]=5;A[5]=6;A[6]=7;A[7]=8;A[8]=9;
    B[0]=9;B[1]=8;B[2]=7;B[3]=6;B[4]=5;B[5]=4;B[6]=3;B[7]=2;B[8]=1;
    tint i=0;
    while(i<3){
        tint j=0;
        while(j<3){
            tint sum=0; tint k=0;
            while(k<3){ sum=sum+A[i*3+k]*B[k*3+j]; k=k+1; }
            C[i*3+j]=sum;
            j=j+1;
        }
        i=i+1;
    }
    return C[0]+C[1]+C[2]+C[3]+C[4]+C[5]+C[6]+C[7]+C[8];
}