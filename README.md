# DurableMultiAgentWorkflowSample

�}���`�G�[�W�F���g�̃��r���[�v���Z�X�� Azure Durable Functions �� Azure OpenAI Service ��p���Ď��������T���v���A�v���P�[�V�����ł��B

## �v���W�F�N�g�̊T�v

���̃T���v���͈ȉ��̎�v�R���|�[�l���g����\������Ă��܂��F

1. **Workflow �T�[�r�X** - Azure Functions �� Azure Durable Functions ���g�p���ă}���`�G�[�W�F���g�̃I�[�P�X�g���[�V�������s���܂�
2. **Windows �N���C�A���g** - WPF �Ŏ������ꂽ�f�X�N�g�b�v�N���C�A���g
3. **SignalR** - ���A���^�C���̒ʐM�Ɏg�p

### �G�[�W�F���g�̃��[�N�t���[

���̃T���v���ł́A�ȉ���3��AI�G�[�W�F���g���g���ĕ��͐����E���r���[�v���Z�X���������Ă��܂��F

- **Writer Agent**: ���[�U�[�̓��͂Ɋ�Â��ĕ��͂𐶐�
- **Reviewer Agent**: �������ꂽ���͂����r���[���ăt�B�[�h�o�b�N���
- **Approver Agent**: ���r���[���ʂɊ�Â��ď��F�܂��͍����߂��𔻒f

## ���s���@

### �O�����

- .NET 9
- Azure OpenAI Service �̃A�N�Z�X
- Azure SignalR Service (Serverless ���[�h)

### AppHost �v���W�F�N�g�̐ݒ�

AppHost �v���W�F�N�g�� .NET Aspire �v���W�F�N�g�ł��B�p�����[�^�[�̐ݒ�� `appsettings.json` �Ȃǂ̐ݒ�t�@�C���ōs���܂��B

`appsettings.json` �̗�:{
  "Parameters": {
    "aoai-endpoint": "<your-aoai-endpoint>",
    "aoai-modeldeploymentname": "<your-model-deployment-name>",
    "signalr-connectionstring": "<your-signalr-connection-string>"
  }
}
�p�����[�^�[�̐���:
- `aoai-endpoint`: Azure OpenAI Service �̃G���h�|�C���g
- `aoai-modeldeploymentname`: Azure OpenAI �Ƀf�v���C�������f���̃f�v���C��
- `signalr-connectionstring`: Serverless ���[�h�� Azure SignalR Service �̐ڑ�������

### �A�v���P�[�V�����̋N���菇

1. **AppHost �v���W�F�N�g�̋N��**:
   - Visual Studio ���� DurableMultiAgentWorkflowSample.AppHost �v���W�F�N�g���N�����邩�A
   - �R�}���h���C������ `dotnet run --project DurableMultiAgentWorkflowSample.AppHost` �����s

2. **Windows �N���C�A���g�̋N��**:
   - AppHost �̋N��������A�����I�� Windows �N���C�A���g���N�����܂�
   - �܂��͎蓮�� DurableMultiAgentWorkflowSample.WindowsClient �v���W�F�N�g�����s���܂�

3. **���[�N�t���[�̎��s**:
   - Windows �N���C�A���g�Ńe�L�X�g����͂��uStart Workflow�v�{�^�����N���b�N����ƁAAI�G�[�W�F���g�ɂ�郌�r���[�v���Z�X���J�n����܂�
   - �i���󋵂̓��A���^�C���ŕ\������A�K�v�ɉ����ă��[�U�[���͂����߂��܂�