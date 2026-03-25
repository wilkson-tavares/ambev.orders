import type { OrderStatus } from '../types/order'

const styles: Record<OrderStatus, string> = {
  Created: 'bg-blue-100 text-blue-700',
  Processing: 'bg-yellow-100 text-yellow-700',
  Sent: 'bg-green-100 text-green-700',
}

const labels: Record<OrderStatus, string> = {
  Created: 'Criado',
  Processing: 'Processando',
  Sent: 'Enviado',
}

interface Props {
  status: OrderStatus
}

export function StatusBadge({ status }: Props) {
  return (
    <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>
      {labels[status]}
    </span>
  )
}
