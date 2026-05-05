export interface OdontogramToothClassification {
  toothTypeLabelKey: string;
  archLabelKey: string;
  quadrantLabelKey: string;
}

export function classifyOdontogramTooth(toothCode: string): OdontogramToothClassification {
  return {
    toothTypeLabelKey: getToothTypeLabelKey(toothCode),
    archLabelKey: getArchLabelKey(toothCode),
    quadrantLabelKey: getQuadrantLabelKey(toothCode)
  };
}

function getToothTypeLabelKey(toothCode: string): string {
  switch (toothCode[1]) {
    case '1':
      return 'Central incisor';
    case '2':
      return 'Lateral incisor';
    case '3':
      return 'Canine';
    case '4':
      return 'First premolar';
    case '5':
      return 'Second premolar';
    case '6':
      return 'First molar';
    case '7':
      return 'Second molar';
    case '8':
      return 'Third molar';
    default:
      return 'Unknown';
  }
}

function getArchLabelKey(toothCode: string): string {
  return toothCode[0] === '1' || toothCode[0] === '2' ? 'Upper' : 'Lower';
}

function getQuadrantLabelKey(toothCode: string): string {
  switch (toothCode[0]) {
    case '1':
      return 'Upper right';
    case '2':
      return 'Upper left';
    case '3':
      return 'Lower left';
    case '4':
      return 'Lower right';
    default:
      return 'Unknown';
  }
}
