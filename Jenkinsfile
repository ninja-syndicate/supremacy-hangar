@Library("shared-library") _
pipeline {
  agent {
    node {
      label 'windows-agent-01'
      customWorkspace "D:\\jenkins-workspace\\supremacy-hangar-${env.BRANCH_NAME}"
    }
  }
  environment {
    deployEnv = mapBranchToDeployEnvironment()
    unityPath = "C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.5f1\\Editor\\Unity.exe"
  }
  stages {
    stage('Build') {
      steps {
        echo 'Sending notification to Slack.'
        slackSend channel: '#test-notifications', 
          color: '#10A90E2',
          message: "Started *supremacy-hangar* build. Job name: *${env.JOB_NAME}*. Build no: *${env.BUILD_NUMBER}*. More info: <${env.BUILD_URL}|supremacy-hangar-build>"

        script {
          if (env.BRANCH_NAME == 'develop') {
              echo 'Prewarm started'
              bat "\"${unityPath}\" -batchmode -quit -buildTarget WebGL -projectPath ${env.WORKSPACE} -logFile prewarm_log.txt -executeMethod BuildSystem.CLI.PreWarm -addressablesLocation staging"
              echo 'Prewarm completed'

              echo 'Build started'
              bat "\"${unityPath}\" -batchmode -quit -buildTarget WebGL -projectPath ${env.WORKSPACE} -logFile builds_log.txt -executeMethod BuildSystem.CLI.BuildWebGL -addressablesLocation staging"
              echo 'Build completed'
          } else {
              echo 'Prewarm started'
              bat "\"${unityPath}\" -batchmode -quit -buildTarget WebGL -projectPath ${env.WORKSPACE} -logFile prewarm_log.txt -executeMethod BuildSystem.CLI.PreWarm -addressablesLocation ${deployEnv}"
              echo 'Prewarm completed'

              echo 'Build started'
              bat "\"${unityPath}\" -batchmode -quit -buildTarget WebGL -projectPath ${env.WORKSPACE} -logFile builds_log.txt -executeMethod BuildSystem.CLI.BuildWebGL -addressablesLocation ${deployEnv}"
              echo 'Build completed'
          }
        }
      }
      post {
        success {
          echo 'Build stage successful.'
          slackSend channel: '#test-notifications',
            color: 'good', 
            message: "*${currentBuild.currentResult}:* *supremacy-hangar* build has *succeded* :innocent:. Job name: *${env.JOB_NAME}*. Build no: *${env.BUILD_NUMBER}*. More info: <${env.BUILD_URL}|supremacy-hangar-build>"
        }
        failure {
          echo 'Build stage unsuccessful.'
          slackSend channel: '#test-notifications',
          color: 'danger', 
          message: "*${currentBuild.currentResult}:* *supremacy-hangar* build has *failed* :astonished:. Job name: *${env.JOB_NAME}*. Build no: *${env.BUILD_NUMBER}*. More info: <${env.BUILD_URL}|supremacy-hangar-build>"
        }
      }
    } 
    stage('Deploy build') {
      steps {
        echo 'Deploy build started'
        bat """
            mkdir Build-to-Deploy
            xcopy /E /Y ${env.WORKSPACE}\\Builds\\WebGL\\Build ${env.WORKSPACE}\\Build-to-Deploy
            xcopy /E /I /Y ${env.WORKSPACE}\\Builds\\WebGL\\StreamingAssets ${env.WORKSPACE}\\Build-to-Deploy\\StreamingAssets
            """
        script {
          if (env.BRANCH_NAME == 'develop') {
              bat "rclone sync \"${env.WORKSPACE}/Build-to-Deploy\" \"afiles:/var/www/html/supremacy-hangar/build/staging/\" --progress --verbose --multi-thread-streams 10"
          } else {
              bat "rclone sync \"${env.WORKSPACE}/Build-to-Deploy\" \"afiles:/var/www/html/supremacy-hangar/build/${deployEnv}/\" --progress --verbose --multi-thread-streams 10"
          }
        }
      }
      post {
          success {
              echo 'Deploy build stage successful.'
              slackSend channel: '#test-notifications',
              color: 'good', 
              message: "*${currentBuild.currentResult}:* *supremacy-hangar* build deploy has *succeded* :innocent:. More info: <${env.BUILD_URL}|supremacy-hangar-deploy>"
          }
          failure {
              echo 'Deploy build stage successful.'
              slackSend channel: '#test-notifications',
                color: 'danger', 
                message: "*${currentBuild.currentResult}:* *supremacy-hangar* build deploy has *failed* :astonished:. More info: <${env.BUILD_URL}|supremacy-hangar-deploy>"
          }
      }
    }
    stage('Deploy addressables') {
      steps {
        echo 'Deploy addressbales started'
         script {
          if (env.BRANCH_NAME == 'develop') {
              bat "rclone sync \"${env.WORKSPACE}/ServerData/\" \"afiles:/var/www/html/supremacy-hangar/addressables/staging/\" --progress --verbose --multi-thread-streams 10"
          } else {
              bat "rclone sync \"${env.WORKSPACE}/ServerData/\" \"afiles:/var/www/html/supremacy-hangar/addressables/${deployEnv}/\" --progress --verbose --multi-thread-streams 10"
          }
        }
        }
        post {
          success {
              echo 'Deploy addressables stage successful.'
              slackSend channel: '#test-notifications',
                color: 'good', 
                message: "*${currentBuild.currentResult}:* *supremacy-hangar* addressables deploy has *succeded* :innocent:. More info: <${env.BUILD_URL}|supremacy-hangar-deploy>"
          }
          failure {
              echo 'Deploy addressables stage successful.'
              slackSend channel: '#test-notifications',
                color: 'danger', 
                message: "*${currentBuild.currentResult}:* *supremacy-hangar* addressables deploy has *failed* :astonished:. More info: <${env.BUILD_URL}|supremacy-hangar-deploy>"
          }
        }
    } 
  }
}