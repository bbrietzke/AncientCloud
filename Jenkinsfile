pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                bat 'dotnet restore'
                bat 'dotnet build --no-restore'
            }
        }
        stage('Test') {
            steps {
                bat 'dotnet test --no-build'
            }
        }
    }
}