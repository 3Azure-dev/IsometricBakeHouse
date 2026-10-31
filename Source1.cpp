#include <iostream>
using namespace std;

// #1 inheritance class get and set values
// int id, age, rollNo;
// string name;
// class Person 
// {
//     private:
//         int id;
//         string name;
//         int age;
//     public:
//         void setValues(int id, string name, int age) {
//             this->id = id;
//             this->name = name;
//             this->age = age;
//         }

//         void getValues() {
//             cout << "ID: " << id << endl;
//             cout << "Name: " << name << endl;
//             cout << "Age: " << age << endl;
//         }

//     };
// class Student : public Person 
// {
//     private:
//         int rollNo;
//     public:

//         void setStudentValues(int rollNo) 
//         {
//             this->rollNo = rollNo;
//         }
//         void getStudentValues() 
//         {
//             cout << "Roll No: " << rollNo << endl;
//             cout << "\n";
//         }
// };

// void main()
// {
//     Student s1;
//     cout << "Enter ID: ";
//     cin >> id;
//     cout << "Enter Name: ";
//     cin >> name;
//     cout << "Enter Age: ";
//     cin >> age;
//     cout << "Enter Roll No: ";
//     cin >> rollNo;
//     s1.setValues(id, name, age);
//     s1.setStudentValues(rollNo);

//     cout << "\nValues of Student S1 are: " << endl;
//     s1.getValues();
//     s1.getStudentValues();
// };

// #2 demonstrate single inheritance using a base class 
int id, age, rollNo;
string name;
class Person
{
protected:
    int id;
    string name;
    int age;
};
class Student : public Person
{
private:
    int rollNo;
public:
public:
    void setValues(int id, string name, int age, int rollNo) {
        this->id = id;
        this->name = name;
        this->age = age;
        this->rollNo = rollNo;
    }

    void getValues() {
        cout << "ID: " << id << endl;
        cout << "Name: " << name << endl;
        cout << "Age: " << age << endl;
        cout << "Roll No: " << rollNo << endl;
        cout << "\n";
    }
};

void main()
{
    Student s1;
    cout << "Enter ID: ";
    cin >> id;
    cout << "Enter Name: ";
    cin >> name;
    cout << "Enter Age: ";
    cin >> age;
    cout << "Enter Roll No: ";
    cin >> rollNo;
    s1.setValues(id, name, age, rollNo);

    cout << "\nValues of Student S1 are: " << endl;
    s1.getValues();

};