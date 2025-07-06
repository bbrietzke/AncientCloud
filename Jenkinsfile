pipeline {
    agent any
    stages {
        stage('Build') {
            steps {
                bat '''
                    dotnet restore
                '''
                sh 'dotnet build --no-restore'
            }
        }
        stage('Test') {
            steps {
                sh 'dotnet test --no-build'
            }
        }
    }
}