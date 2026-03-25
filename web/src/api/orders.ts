import axios from 'axios'
import type { Order, CreateOrderResponse, OrderStatus, OrderItem } from '../types/order'

const http = axios.create({ baseURL: '/api' })

export interface CreateOrderPayload {
  orderId: number
  clientId: number
  items: OrderItem[]
}

export const ordersApi = {
  create: (payload: CreateOrderPayload) =>
    http.post<CreateOrderResponse>('/orders', payload).then((r) => r.data),

  getById: (id: number) =>
    http.get<Order>(`/orders/${id}`).then((r) => r.data),

  listByStatus: (status: OrderStatus) =>
    http.get<Order[]>('/orders', { params: { status } }).then((r) => r.data),

  startProcessing: (id: number) =>
    http.patch<Order>(`/orders/${id}/start-processing`).then((r) => r.data),

  send: (id: number) =>
    http.patch<Order>(`/orders/${id}/send`).then((r) => r.data),
}
